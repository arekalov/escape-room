using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public Image highlight;

    public static int HoveredSlot { get; private set; } = -1;

    int _index;

    public void Init(int index)
    {
        _index = index;
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            var mgr = InventoryManager.Instance;
            if (mgr == null) return;
            mgr.SelectSlot(mgr.SelectedSlot == _index ? -1 : _index);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var items = InventoryManager.Instance?.GetItems();
        if (items != null && _index < items.Count)
            HoveredSlot = _index;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HoveredSlot == _index) HoveredSlot = -1;
    }

    void OnDisable() { if (HoveredSlot == _index) HoveredSlot = -1; }

    public void Refresh(ItemData item, bool selected)
    {
        bool hasItem = item != null;
        iconImage.enabled = hasItem;
        if (hasItem) iconImage.sprite = item.icon;
        highlight.color = selected
            ? new Color(1f, 0.85f, 0.2f, 0.85f)
            : new Color(1f, 1f, 1f, 0f);
    }
}
