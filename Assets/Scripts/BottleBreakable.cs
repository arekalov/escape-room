using System.Collections;
using UnityEngine;

// Бутылка в углу: при использовании кружкой разбивается, появляется зажигалка
public class BottleBreakable : MonoBehaviour, IInteractable
{
    [Header("Required item")]
    public ItemData mugItem;

    [Header("What spawns after break")]
    public GameObject lighterObject; // уже в сцене, но inactive

    [Header("Optional break effect")]
    public GameObject breakEffectPrefab;
    public float      hideDelay = 0.15f;

    bool _broken;

    public void Interact(ItemData usedItem)
    {
        if (_broken) return;
        if (usedItem == null || usedItem != mugItem) return;

        _broken = true;
        InventoryManager.Instance?.RemoveItem(mugItem);

        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

        if (lighterObject != null)
        {
            lighterObject.transform.position = transform.position + Vector3.up * 0.05f;
            lighterObject.SetActive(true);
        }

        StartCoroutine(HideAfterDelay());
    }

    public string GetHintText(ItemData usedItem)
    {
        if (_broken) return "";
        return (usedItem != null && usedItem == mugItem)
            ? "[F] Разбить кружкой"
            : "[E] Осмотреть бутылку";
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        gameObject.SetActive(false);
    }
}
