using System.Collections;
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

    void Start() => StartCoroutine(Play());

    IEnumerator Play()
    {
        playerController.enabled = false;
        SetAlpha(1f);

        cameraRoot.localPosition    = new Vector3(0f, lyingCameraY, 0f);
        cameraRoot.localEulerAngles = new Vector3(lyingPitch, 0f, 0f);

        yield return new WaitForSeconds(initialDarkness);

        // Моргание — как при открытии глаз
        for (int i = 0; i < blinkCount; i++)
        {
            yield return Fade(1f, 0.08f, blinkOpenTime);
            yield return Fade(0.08f, 1f, blinkCloseTime);
        }

        // Открытие глаз + подъём одновременно
        StartCoroutine(Fade(1f, 0f, finalOpenDuration));
        yield return StandUp(standUpDuration);

        yield return new WaitForSeconds(0.4f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        playerController.enabled = true;

        GameManager.Instance?.OnCutsceneDone();

        if (eyeOverlay) Destroy(eyeOverlay.transform.root.gameObject);
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

    IEnumerator StandUp(float duration)
    {
        const float standingY = 1.5f;
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

    void SetAlpha(float a)
    {
        if (eyeOverlay) eyeOverlay.color = new Color(0f, 0f, 0f, a);
    }
}
