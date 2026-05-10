using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public int slotCount = 6;

    readonly List<ItemData>                    _items   = new List<ItemData>();
    readonly Dictionary<ItemData, GameObject>  _sources = new Dictionary<ItemData, GameObject>();

    public event Action    OnInventoryChanged;
    public event Action<int> OnSlotSelected;

    public int SelectedSlot { get; private set; } = -1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool AddItem(ItemData item, GameObject source = null)
    {
        if (_items.Count >= slotCount) return false;
        _items.Add(item);
        if (source != null) _sources[item] = source;
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void DropItem(int index, Vector3 worldPosition, Vector3 throwVelocity)
    {
        if (index < 0 || index >= _items.Count) return;
        var item = _items[index];
        if (_sources.TryGetValue(item, out var go) && go != null)
        {
            go.transform.position = worldPosition;
            go.transform.rotation = UnityEngine.Random.rotation;
            go.SetActive(true);
            _sources.Remove(item);

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.isKinematic   = false;
            rb.linearVelocity     = throwVelocity;
            rb.angularVelocity = new Vector3(
                UnityEngine.Random.Range(-8f, 8f),
                UnityEngine.Random.Range(-8f, 8f),
                UnityEngine.Random.Range(-8f, 8f));

            StartCoroutine(MakeKinematicWhenStopped(rb));
        }
        RemoveAt(index);
        SelectSlot(-1);
    }

    IEnumerator MakeKinematicWhenStopped(Rigidbody rb)
    {
        yield return new WaitForSeconds(0.3f);
        while (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
            yield return null;
        if (rb != null) rb.isKinematic = true;
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
