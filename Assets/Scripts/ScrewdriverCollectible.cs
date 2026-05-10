using UnityEngine;

// Замени Collectible на этот скрипт на объекте отвёртки в сейфе
public class ScrewdriverCollectible : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public void Interact(ItemData usedItem)
    {
        if (InventoryManager.Instance.AddItem(itemData, gameObject))
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            gameObject.SetActive(false);
            GameManager.Instance?.OnScrewdriverPickedUp();
        }
    }

    public string GetHintText(ItemData usedItem) => $"[E] Взять {itemData?.itemName}";
}
