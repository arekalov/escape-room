using System.Collections;
using UnityEngine;
using TMPro;

public class TVController : MonoBehaviour
{
    [Header("Screen material slot (index in MeshRenderer)")]
    public int screenMaterialIndex = 2;

    [Header("Materials")]
    public Material matOff;
    public Material matNoise;
    public Material matCode;

    [Header("Overlay (child World Space Canvas)")]
    [SerializeField] string screenCanvasName = "TVScreenCanvas";
    [SerializeField] float codeFadeInDuration = 1.2f;
    [SerializeField] float codeFadeOutDuration = 0.45f;
    [Tooltip("Лёгкое «дыхание» яркости экрана текста (альфа CanvasGroup), имитация ЭЛТ")]
    [SerializeField] float codePulseAmplitude = 0.055f;
    [SerializeField] float codePulseSpeed = 2f;

    public enum TVState { Off, Noise, Code }
    public TVState State { get; private set; } = TVState.Off;

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

    static float SmoothStep01(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    public void TurnOn()
    {
        if (State != TVState.Off) return;
        SetState(TVState.Noise);
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
            float k = EaseOutCubic(t / dur);
            if (_canvasGroup != null)
                _canvasGroup.alpha = k;
            yield return null;
        }

        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        float amp = Mathf.Clamp(codePulseAmplitude, 0f, 0.35f);
        while (enabled && State == TVState.Code && _screenCanvas != null && _screenCanvas.activeInHierarchy)
        {
            if (_canvasGroup != null)
            {
                float flicker = 1f - amp + amp * Mathf.Sin(Time.time * codePulseSpeed);
                _canvasGroup.alpha = Mathf.Clamp01(flicker);
            }
            yield return null;
        }
    }

    public void TurnOff()
    {
        if (State == TVState.Off) return;
        SetupOverlay();
        StartCoroutine(TurnOffRoutine());
    }

    IEnumerator TurnOffRoutine()
    {
        if (_textRoutine != null)
        {
            StopCoroutine(_textRoutine);
            _textRoutine = null;
        }

        if (_canvasGroup != null && _screenCanvas != null && _screenCanvas.activeSelf)
        {
            float start = _canvasGroup.alpha;
            float dur = Mathf.Max(0.08f, codeFadeOutDuration);
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float u = SmoothStep01(t / dur);
                _canvasGroup.alpha = Mathf.Lerp(start, 0f, u);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }

        SetState(TVState.Off);
        if (_screenCanvas != null)
            _screenCanvas.SetActive(false);

        GameManager.Instance?.OnTVTurnedOff();
    }

    void SetState(TVState newState)
    {
        State = newState;
        if (_renderer == null)
            _renderer = GetComponent<MeshRenderer>();
        if (_renderer == null)
            return;

        var mats = _renderer.sharedMaterials;
        mats[screenMaterialIndex] = newState switch
        {
            TVState.Off => matOff,
            TVState.Noise => matNoise,
            TVState.Code => matCode,
            _ => matOff
        };
        _renderer.sharedMaterials = mats;
    }
}
