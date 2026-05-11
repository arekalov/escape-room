using System.Collections;
using UnityEngine;

public class BottleBreakable : MonoBehaviour, IInteractable
{
    [Header("Required item")]
    public ItemData mugItem;

    [Header("What spawns after break")]
    public GameObject lighterObject;

    [Header("Optional break effect")]
    public GameObject breakEffectPrefab;

    bool _broken;

    public void Interact(ItemData usedItem)
    {
        if (_broken) return;
        if (usedItem == null || usedItem != mugItem) return;

        _broken = true;
        InventoryManager.Instance?.RemoveItem(mugItem);
        AudioManager.PlayBottle();

        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

        if (lighterObject != null)
        {
            lighterObject.transform.position = transform.position + Vector3.up * 0.05f;
            lighterObject.SetActive(true);
        }

        StartCoroutine(BreakAnimation());
    }

    public string GetHintText(ItemData usedItem)
    {
        if (_broken) return "";
        return (usedItem != null && usedItem == mugItem)
            ? "[F] Разбить кружкой"
            : "";
    }

    IEnumerator BreakAnimation()
    {
        Vector3 origPos = transform.position;
        Quaternion origRot = transform.localRotation;

        // Rapid shake (impact shudder)
        for (float t = 0f; t < 0.12f; t += Time.deltaTime)
        {
            float shake = Mathf.Sin(t / 0.12f * Mathf.PI * 8f) * 0.025f * (1f - t / 0.12f);
            transform.position = origPos + new Vector3(shake, 0f, shake * 0.5f);
            float tilt = Mathf.Sin(t / 0.12f * Mathf.PI * 6f) * 8f * (1f - t / 0.12f);
            transform.localRotation = origRot * Quaternion.Euler(0f, 0f, tilt);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
