using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public int slotCount = 6;

    readonly List<ItemData> _items = new List<ItemData>();

    public event Action OnInventoryChanged;
    public event Action<int> OnSlotSelected;

    public int SelectedSlot { get; private set; } = -1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool AddItem(ItemData item)
    {
        if (_items.Count >= slotCount) return false;
        _items.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _items.Count) return;
        _items.RemoveAt(index);
        if (SelectedSlot >= _items.Count) SelectSlot(_items.Count - 1);
        OnInventoryChanged?.Invoke();
    }

    public void SelectSlot(int index)
    {
        SelectedSlot = (index >= 0 && index < _items.Count) ? index : -1;
        OnSlotSelected?.Invoke(SelectedSlot);
    }

    public ItemData GetSelected() =>
        SelectedSlot >= 0 && SelectedSlot < _items.Count ? _items[SelectedSlot] : null;

    public bool HasItem(ItemData item) => _items.Contains(item);

    public void RemoveItem(ItemData item)
    {
        int i = _items.IndexOf(item);
        if (i >= 0) RemoveAt(i);
    }

    public List<ItemData> GetItems() => _items;
}
