using UnityEngine;

public class NumpadButton : MonoBehaviour
{
    public string value;

    public void OnClick()
    {
        var panel = CodeInputPanel.Instance;
        if (panel == null) return;

        if (value == "backspace")
            panel.PressBackspace();
        else if (value == "submit")
            panel.PressSubmit();
        else
            panel.PressDigit(value);
    }
}
