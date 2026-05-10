using System.Collections;
using UnityEngine;

// Книга: при использовании зажигалкой сгорает, появляется ключ на/у сейфа
public class BookBurnable : MonoBehaviour, IInteractable
{
    [Header("Required item")]
    public ItemData lighterItem;

    [Header("Key to reveal after burn")]
    public GameObject keyObject; // inactive GO — активируем после сгорания

    [Header("Optional burn effect")]
    public Animator bookAnimator;         // триггер "Burn"
    public string   burnTrigger = "Burn";
    public GameObject burnParticlePrefab;
    public float    burnDuration = 2f;    // сколько ждать до скрытия

    bool _burned;

    public void Interact(ItemData usedItem)
    {
        if (_burned) return;
        if (usedItem == null || usedItem != lighterItem) return;

        _burned = true;
        InventoryManager.Instance?.RemoveItem(lighterItem);

        if (bookAnimator != null && !string.IsNullOrEmpty(burnTrigger))
            bookAnimator.SetTrigger(burnTrigger);

        if (burnParticlePrefab != null)
            Instantiate(burnParticlePrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity);

        StartCoroutine(BurnRoutine());
    }

    public string GetHintText(ItemData usedItem)
    {
        if (_burned) return "";
        return (usedItem != null && usedItem == lighterItem)
            ? "[F] Поджечь зажигалкой"
            : "[E] Осмотреть книгу";
    }

    IEnumerator BurnRoutine()
    {
        yield return new WaitForSeconds(burnDuration);

        if (keyObject != null) keyObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
