using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    public Image highlight;

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
