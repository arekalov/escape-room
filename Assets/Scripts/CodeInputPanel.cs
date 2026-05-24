using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CodeInputPanel : MonoBehaviour
{
    public static CodeInputPanel Instance { get; private set; }

    [Header("UI References")]
    public TMP_InputField inputField;
    public TextMeshProUGUI errorText;
    public CanvasGroup canvasGroup;

    // Цифры читаем сами — обходим EventSystem
    static readonly (Key key, char ch)[] _digitKeys =
    {
        (Key.Digit0, '0'), (Key.Digit1, '1'), (Key.Digit2, '2'),
        (Key.Digit3, '3'), (Key.Digit4, '4'), (Key.Digit5, '5'),
        (Key.Digit6, '6'), (Key.Digit7, '7'), (Key.Digit8, '8'),
        (Key.Digit9, '9'),
        (Key.Numpad0, '0'), (Key.Numpad1, '1'), (Key.Numpad2, '2'),
        (Key.Numpad3, '3'), (Key.Numpad4, '4'), (Key.Numpad5, '5'),
        (Key.Numpad6, '6'), (Key.Numpad7, '7'), (Key.Numpad8, '8'),
        (Key.Numpad9, '9'),
    };

    string _entered = "";
    bool   _open;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open()
    {
        if (_open) return;
        _open = true;
        _entered = "";
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        RefreshDisplay();
        if (errorText) errorText.text = "";
    }

    public void Close()
    {
        if (!_open) return;
        _open = false;
        gameObject.SetActive(false);
    }

    // Вызываются кнопками numpad
    public void PressDigit(string digit)
    {
        if (!_open || _entered.Length >= 8) return;
        _entered += digit;
        RefreshDisplay();
    }

    public void PressBackspace()
    {
        if (!_open || _entered.Length == 0) return;
        _entered = _entered[..^1];
        RefreshDisplay();
    }

    public void PressSubmit()
    {
        if (!_open) return;
        Submit();
    }

    void Update()
    {
        if (!_open) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame) { Close(); return; }

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        { Submit(); return; }

        if (kb.backspaceKey.wasPressedThisFrame && _entered.Length > 0)
        {
            _entered = _entered[..^1];
            RefreshDisplay();
            return;
        }

        foreach (var (key, ch) in _digitKeys)
        {
            if (kb[key].wasPressedThisFrame && _entered.Length < 8)
            {
                _entered += ch;
                RefreshDisplay();
                break;
            }
        }
    }

    void RefreshDisplay()
    {
        if (inputField == null) return;
        inputField.SetTextWithoutNotify(_entered);

        if (inputField.placeholder is TextMeshProUGUI ph)
            ph.gameObject.SetActive(_entered.Length == 0);
    }

    void Submit()
    {
        if (_entered == GameManager.SecretCode)
        {
            GameManager.Instance?.OnCodeCorrect();
            Close();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ShowError());
        }
    }

    IEnumerator ShowError()
    {
        if (errorText) errorText.text = "Неверный код!";
        _entered = "";
        RefreshDisplay();
        yield return new WaitForSeconds(1.5f);
        if (errorText) errorText.text = "";
    }

    void SetPlayerControlsEnabled(bool on)
    {
        var vpi = FindFirstObjectByType<VRPlayerInteraction>();
        if (vpi) vpi.enabled = on;

        var inv = FindFirstObjectByType<InventoryUI>();
        if (inv) inv.enabled = on;
    }
}
