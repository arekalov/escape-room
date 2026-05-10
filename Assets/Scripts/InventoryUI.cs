using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public List<InventorySlot> slots;

void Start()
    {
        for (int i = 0; i < slots.Count; i++)
            slots[i].Init(i);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += Refresh;
            InventoryManager.Instance.OnSlotSelected    += _ => Refresh();
        }
        Refresh();
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
            InventoryManager.Instance.OnSlotSelected    -= _ => Refresh();
        }
    }

    void Refresh()
    {
        var items = InventoryManager.Instance.GetItems();
        int sel   = InventoryManager.Instance.SelectedSlot;
        for (int i = 0; i < slots.Count; i++)
        {
            var item = i < items.Count ? items[i] : null;
            slots[i].Refresh(item, i == sel);
        }
    }
}
