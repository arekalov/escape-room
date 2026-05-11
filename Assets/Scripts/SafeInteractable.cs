using UnityEngine;

// Сейф: открывается ключом, внутри отвёртка
public class SafeInteractable : MonoBehaviour, IInteractable
{
    [Header("Required item")]
    public ItemData keyItem;

    [Header("Animation")]
    public Animator safeAnimator;
    public string   openTrigger = "Open";

    [Header("What's inside (activate after open)")]
    public GameObject screwdriverObject; // Collectible GO — inactive по умолчанию

    bool _opened;

    public void Interact(ItemData usedItem)
    {
        if (_opened) return;
        if (usedItem == null || usedItem != keyItem) return;

        _opened = true;
        InventoryManager.Instance?.RemoveItem(keyItem);

        AudioManager.PlaySafeDoor();
        if (safeAnimator != null) safeAnimator.SetTrigger(openTrigger);
        if (screwdriverObject != null) screwdriverObject.SetActive(true);
    }

    public string GetHintText(ItemData usedItem)
    {
        if (_opened) return "";
        return (usedItem != null && usedItem == keyItem)
            ? "[F] Открыть ключом"
            : "Сейф заперт";
    }
}
