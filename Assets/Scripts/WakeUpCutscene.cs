using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class WakeUpCutscene : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController playerController;
    public Transform             cameraRoot;
    public Image                 eyeOverlay;

    [Header("Camera — lying position")]
    public float lyingCameraY = 0.25f;
    public float lyingPitch   = 22f;

    [Header("Timing (seconds)")]
    public float initialDarkness   = 1.0f;
    public float blinkOpenTime     = 0.12f;
    public float blinkCloseTime    = 0.08f;
    public int   blinkCount        = 2;
    public float finalOpenDuration = 2.0f;
    public float standUpDuration   = 2.5f;

    void Awake()
    {
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null && cameraRoot == null)
            cameraRoot = playerController.cameraRoot;

        if (eyeOverlay == null)
        {
            var go = GameObject.Find("EyeOverlay");
            if (go) eyeOverlay = go.GetComponent<Image>();
        }
    }

    bool _isVR;
    bool _playing;

    void Start()
    {
        _isVR = (playerController == null);

        if (_isVR)
        {
            var xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin != null && xrOrigin.CameraFloorOffsetObject != null)
                cameraRoot = xrOrigin.CameraFloorOffsetObject.transform;
        }

        SetAlpha(0f);

        // Ждём Main Menu; если меню нет — сразу кат-сцена (отладка / старая сцена).
        var menu = FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include);
        if (menu == null || !menu.isActiveAndEnabled)
            Begin();
    }

    /// <summary>Вызывается из MainMenuController после нажатия Start.</summary>
    public void Begin()
    {
        if (_playing) return;
        _playing = true;
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        if (playerController != null) playerController.enabled = false;
        SetAlpha(1f);

        if (_isVR)
        {
            // VR: only animate Y (no rotation — TrackedPoseDriver controls camera angle)
            float standingY = cameraRoot != null ? cameraRoot.localPosition.y : 1.5f;
            if (cameraRoot != null)
                cameraRoot.localPosition = new Vector3(0f, lyingCameraY, 0f);

            yield return new WaitForSeconds(initialDarkness);
            AudioManager.PlayStandUp();

            StartCoroutine(Fade(1f, 0f, finalOpenDuration));
            if (cameraRoot != null) yield return StandUpVR(standUpDuration, standingY);
            else yield return new WaitForSeconds(finalOpenDuration);
        }
        else
        {
            // Non-VR: full animation with rotation
            float standingY = cameraRoot != null ? cameraRoot.localPosition.y : 1.5f;
            if (cameraRoot != null)
            {
                cameraRoot.localPosition    = new Vector3(0f, lyingCameraY, 0f);
                cameraRoot.localEulerAngles = new Vector3(lyingPitch, 0f, 0f);
            }

            yield return new WaitForSeconds(initialDarkness);
            AudioManager.PlayStandUp();

            for (int i = 0; i < blinkCount; i++)
            {
                yield return Fade(1f, 0.08f, blinkOpenTime);
                yield return Fade(0.08f, 1f, blinkCloseTime);
            }

            StartCoroutine(Fade(1f, 0f, finalOpenDuration));
            if (cameraRoot != null) yield return StandUp(standUpDuration, standingY);
            else yield return new WaitForSeconds(finalOpenDuration);
        }

        yield return new WaitForSeconds(0.4f);

        if (playerController != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            playerController.enabled = true;
        }

        GameManager.Instance?.OnCutsceneDone();

        if (eyeOverlay)
        {
            var canvas = eyeOverlay.GetComponentInParent<Canvas>();
            Destroy(canvas != null ? canvas.gameObject : eyeOverlay.gameObject);
        }
        Destroy(gameObject);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    IEnumerator StandUp(float duration, float standingY)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, t / duration);
            cameraRoot.localPosition    = new Vector3(0f, Mathf.Lerp(lyingCameraY, standingY, s), 0f);
            cameraRoot.localEulerAngles = new Vector3(Mathf.Lerp(lyingPitch, 0f, s), 0f, 0f);
            yield return null;
        }
        cameraRoot.localPosition    = new Vector3(0f, standingY, 0f);
        cameraRoot.localEulerAngles = Vector3.zero;
    }

    IEnumerator StandUpVR(float duration, float standingY)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, t / duration);
            cameraRoot.localPosition = new Vector3(0f, Mathf.Lerp(lyingCameraY, standingY, s), 0f);
            yield return null;
        }
        cameraRoot.localPosition = new Vector3(0f, standingY, 0f);
    }

    void SetAlpha(float a)
    {
        if (eyeOverlay) eyeOverlay.color = new Color(0f, 0f, 0f, a);
    }
}
