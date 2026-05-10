using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Прикрепи на trigger-коллайдер у выхода за дверью
public class VictoryTrigger : MonoBehaviour
{
    [Header("Victory screen")]
    public CanvasGroup victoryPanel;
    public float       fadeDuration = 2f;

    bool _triggered;

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player") && other.GetComponentInParent<FirstPersonController>() == null) return;

        _triggered = true;
        StartCoroutine(ShowVictory());
    }

    IEnumerator ShowVictory()
    {
        // Отключить управление игрока
        var fpc = FindFirstObjectByType<FirstPersonController>();
        if (fpc != null) fpc.enabled = false;

        if (victoryPanel != null)
        {
            victoryPanel.gameObject.SetActive(true);
            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                victoryPanel.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            victoryPanel.alpha = 1f;
        }

        yield return new WaitForSeconds(4f);

        // Можно добавить: UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
