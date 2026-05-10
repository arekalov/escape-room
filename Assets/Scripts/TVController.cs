using System.Collections;
using UnityEngine;
using TMPro;

public class TVController : MonoBehaviour
{
    [Header("Screen material slot (index in MeshRenderer)")]
    public int screenMaterialIndex = 2;

    [Header("Materials")]
    public Material matNoise;
    public Material matCode;

    [Header("Overlay (child World Space Canvas)")]
    [SerializeField] string screenCanvasName = "TVScreenCanvas";
    [SerializeField] float codeFadeInDuration = 1.2f;
    [SerializeField] float codePulseAmplitude = 0.055f;
    [SerializeField] float codePulseSpeed = 2f;

    public enum TVState { Noise, Code }
    public TVState State { get; private set; } = TVState.Noise;

    MeshRenderer _renderer;
    GameObject _screenCanvas;
    CanvasGroup _canvasGroup;
    TextMeshProUGUI _codeText;
    Coroutine _textRoutine;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        SetupOverlay();
    }

    void Start()
    {
        SetState(TVState.Noise);
    }

    void SetupOverlay()
    {
        if (_screenCanvas != null) return;
        Transform canvasTx = transform.Find(screenCanvasName);
        if (canvasTx == null) return;

        _screenCanvas = canvasTx.gameObject;
        _canvasGroup = _screenCanvas.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = _screenCanvas.AddComponent<CanvasGroup>();

        _codeText = _screenCanvas.GetComponentInChildren<TextMeshProUGUI>(true);
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        SyncTextColorOpaque();
    }

    void SyncTextColorOpaque()
    {
        if (_codeText == null) return;
        Color c = _codeText.color;
        c.a = 1f;
        _codeText.color = c;
    }

    static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t) * (1f - t);
    }

    public void ShowCode()
    {
        SetupOverlay();
        SetState(TVState.Code);

        if (_screenCanvas == null) return;

        if (_textRoutine != null)
        {
            StopCoroutine(_textRoutine);
            _textRoutine = null;
        }

        _screenCanvas.SetActive(true);
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;

        SyncTextColorOpaque();
        _textRoutine = StartCoroutine(RevealAndPulseRoutine());
    }

    IEnumerator RevealAndPulseRoutine()
    {
        float dur = Mathf.Max(0.08f, codeFadeInDuration);
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (_canvasGroup != null)
                _canvasGroup.alpha = EaseOutCubic(t / dur);
            yield return null;
        }
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        float amp = Mathf.Clamp(codePulseAmplitude, 0f, 0.35f);
        while (enabled && State == TVState.Code)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Clamp01(1f - amp + amp * Mathf.Sin(Time.time * codePulseSpeed));
            yield return null;
        }
    }

    void SetState(TVState newState)
    {
        State = newState;
        if (_renderer == null)
            _renderer = GetComponent<MeshRenderer>();
        if (_renderer == null) return;

        var mats = _renderer.sharedMaterials;
        mats[screenMaterialIndex] = newState == TVState.Code ? matCode : matNoise;
        _renderer.sharedMaterials = mats;

        if (newState == TVState.Noise && _screenCanvas != null)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            _screenCanvas.SetActive(false);
        }
    }
}
