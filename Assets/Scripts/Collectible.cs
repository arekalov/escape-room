using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public void Interact(ItemData usedItem)
    {
        if (InventoryManager.Instance.AddItem(itemData, gameObject))
            gameObject.SetActive(false);
    }

    public string GetHintText(ItemData usedItem) =>
        $"[E] Взять {itemData?.itemName}";
}
