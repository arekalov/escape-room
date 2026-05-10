using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum RadioLine
{
    Intro,      // "НКВД едут, надо выбраться. Посмотри сначала телевизор"
    FindDoor,   // "Открой дверь и выбирайся"
    TurnOffTV,  // "Выключи телевизор — и дверь откроется"
    Win         // "Беги!"
}

public class RadioSubtitles : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup panelGroup;
    public Text        subtitleText;

    [Header("Timing")]
    public float fadeSpeed    = 4f;
    public float displayTime  = 5f;

    [Header("Lines (Russian)")]
    public string lineIntro    = "Срочно выбирайся! Скоро приедет НКВД.\nА пока — посмотри телевизор.";
    public string lineFindDoor = "Открой дверь и скорее выбирайся отсюда!";
    public string lineTurnOff  = "Выключи телевизор — и дверь откроется.";
    public string lineWin      = "Ты свободен!";

    Coroutine _current;

    public void PlayLine(RadioLine line)
    {
        string text = line == RadioLine.Intro    ? lineIntro    :
                      line == RadioLine.FindDoor ? lineFindDoor :
                      line == RadioLine.TurnOffTV? lineTurnOff  :
                                                   lineWin;

        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(ShowRoutine(text));
    }

    IEnumerator ShowRoutine(string text)
    {
        if (subtitleText) subtitleText.text = text;

        // Fade in
        while (panelGroup != null && panelGroup.alpha < 1f)
        {
            panelGroup.alpha = Mathf.MoveTowards(panelGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(displayTime);

        // Fade out
        while (panelGroup != null && panelGroup.alpha > 0f)
        {
            panelGroup.alpha = Mathf.MoveTowards(panelGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
