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
    Collider _col;

    void Awake() => _col = GetComponent<Collider>();

    void Update()
    {
        if (_triggered) return;
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;
        var checkPos = player.transform.position + Vector3.up * 0.8f;
        if (_col != null && _col.bounds.Contains(checkPos))
            Fire();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player") && other.GetComponentInParent<FirstPersonController>() == null) return;
        Fire();
    }

    void OnTriggerStay(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player") && other.GetComponentInParent<FirstPersonController>() == null) return;
        Fire();
    }

    void Fire()
    {
        if (_triggered) return;
        _triggered = true;
        StartCoroutine(ShowVictory());
    }

    IEnumerator ShowVictory()
    {
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
    }
}
