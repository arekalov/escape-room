using System.Collections.Generic;
using UnityEngine.InputSystem;
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

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) Toggle(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) Toggle(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) Toggle(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) Toggle(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) Toggle(4);
    }

    void Toggle(int index)
    {
        var mgr = InventoryManager.Instance;
        if (mgr == null) return;
        mgr.SelectSlot(mgr.SelectedSlot == index ? -1 : index);
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
