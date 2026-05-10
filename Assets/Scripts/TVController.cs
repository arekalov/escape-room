using System.Collections;
using UnityEngine;
using TMPro;

public class TVController : MonoBehaviour, IInteractable
{
    [Header("Screen material index in MeshRenderer")]
    public int screenMaterialIndex = 2;

    [Header("Materials")]
    public Material matOff;
    public Material matNoise;
    public Material matCode;

    [Header("Screen canvas (World Space, child of TV)")]
    public string screenCanvasName = "TVScreenCanvas";
    public float  codeFadeInDuration = 1.2f;
    public float  codePulseAmplitude = 0.055f;
    public float  codePulseSpeed     = 2f;

    public enum TVState { Off, Noise, Fixed, Code, TurnedOff }
    public TVState State { get; private set; } = TVState.Noise;

    MeshRenderer    _renderer;
    GameObject      _screenCanvas;
    CanvasGroup     _canvasGroup;
    TextMeshProUGUI _codeText;
    Coroutine       _textRoutine;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        SetupOverlay();
    }

    void Start() => SetState(TVState.Noise);

    // ── IInteractable ──────────────────────────────────────────
    public void Interact(ItemData usedItem)
    {
        if (State == TVState.Fixed)
        {
            SetState(TVState.Code);
            SetupOverlay();
            if (_screenCanvas != null)
            {
                if (_textRoutine != null) { StopCoroutine(_textRoutine); _textRoutine = null; }
                _screenCanvas.SetActive(true);
                if (_canvasGroup) _canvasGroup.alpha = 0f;
                SyncTextOpaque();
                _textRoutine = StartCoroutine(RevealAndPulse());
            }
        }
        else if (State == TVState.Code)
        {
            SetState(TVState.TurnedOff);
            GameManager.Instance?.OnTVTurnedOff();
        }
    }

    public string GetHintText(ItemData usedItem)
    {
        return State == TVState.Fixed    ? "[E] Включить телевизор"  :
               State == TVState.Noise    ? "Ремонтируется…"           :
               State == TVState.Code     ? "[E] Выключить телевизор"  :
                                           "";
    }

    // ── вызывается GameManager после второй починки ───────────
    public void ShowCode()
    {
        SetupOverlay();
        SetState(TVState.Fixed);
    }

    // ── helpers ────────────────────────────────────────────────
    void SetState(TVState s)
    {
        State = s;
        ApplyMaterial(s);

        if (s != TVState.Code && _screenCanvas != null)
        {
            if (_canvasGroup) _canvasGroup.alpha = 0f;
            _screenCanvas.SetActive(false);
        }
    }

    void ApplyMaterial(TVState s)
    {
        if (_renderer == null) return;
        var mats = _renderer.sharedMaterials;
        if (screenMaterialIndex >= mats.Length) return;
        mats[screenMaterialIndex] = s == TVState.Code   ? matCode  :
                                    s == TVState.Noise  ? matNoise :
                                    matOff ?? matNoise;
        _renderer.sharedMaterials = mats;
    }

    void SetupOverlay()
    {
        if (_screenCanvas != null) return;
        var tx = transform.Find(screenCanvasName);
        if (tx == null) return;
        _screenCanvas = tx.gameObject;
        _canvasGroup  = _screenCanvas.GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = _screenCanvas.AddComponent<CanvasGroup>();
        _codeText = _screenCanvas.GetComponentInChildren<TextMeshProUGUI>(true);
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
        SyncTextOpaque();
    }

    void SyncTextOpaque()
    {
        if (_codeText == null) return;
        var c = _codeText.color; c.a = 1f; _codeText.color = c;
    }

    IEnumerator RevealAndPulse()
    {
        float dur = Mathf.Max(0.08f, codeFadeInDuration);
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            if (_canvasGroup) _canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t / dur);
            yield return null;
        }
        if (_canvasGroup) _canvasGroup.alpha = 1f;

        float amp = Mathf.Clamp(codePulseAmplitude, 0f, 0.35f);
        while (enabled && State == TVState.Code)
        {
            if (_canvasGroup)
                _canvasGroup.alpha = Mathf.Clamp01(1f - amp + amp * Mathf.Sin(Time.time * codePulseSpeed));
            yield return null;
        }
    }
}
