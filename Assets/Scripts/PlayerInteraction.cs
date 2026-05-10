using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 1.5f;
    public Transform cam;

    [Header("Main hint (E / F)")]
    public CanvasGroup hintGroup;
    public Text        hintText;

    [Header("Drop hint (Q)")]
    public CanvasGroup dropHintGroup;
    public Text        dropHintText;

    [Header("Use hint (F on target)")]
    public CanvasGroup useHintGroup;
    public Text        useHintText;

    [Header("Animation")]
    public float fadeSpeed = 10f;

    int           _mask;
    IInteractable _target;

    void Awake()
    {
        _mask = ~(1 << gameObject.layer);
        SetAlpha(hintGroup,    0f);
        SetAlpha(dropHintGroup, 0f);
        SetAlpha(useHintGroup,  0f);
    }

    void Update()
    {
        Scan();
        HandleInput();
        UpdateHints();
    }

    void Scan()
    {
        var origin = cam ? cam : Camera.main.transform;
        var hits   = Physics.RaycastAll(origin.position, origin.forward, interactRange, _mask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        _target = null;
        foreach (var hit in hits)
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null) { _target = interactable; break; }
        }
    }

    void HandleInput()
    {
        if (Keyboard.current == null) return;
        var mgr = InventoryManager.Instance;

        // E — base interaction (pick up, activate)
        if (Keyboard.current.eKey.wasPressedThisFrame && _target != null)
            _target.Interact(null);

        // F — use selected item on target
        if (Keyboard.current.fKey.wasPressedThisFrame && _target != null && mgr?.GetSelected() != null)
            _target.Interact(mgr.GetSelected());

        // Q — drop selected item
        if (Keyboard.current.qKey.wasPressedThisFrame && mgr != null && mgr.SelectedSlot >= 0)
        {
            var origin   = cam ? cam : Camera.main.transform;
            var dropPos  = origin.position + origin.forward * 0.6f;
            var throwVel = origin.forward * 3f + Vector3.up * 1f;
            mgr.DropItem(mgr.SelectedSlot, dropPos, throwVel);
        }
    }

    void UpdateHints()
    {
        var mgr      = InventoryManager.Instance;
        var selected = mgr?.GetSelected();

        // Main hint — E action
        if (_target != null)
        {
            if (hintText) hintText.text = _target.GetHintText(null);
            FadeTo(hintGroup, 1f);
        }
        else
        {
            FadeTo(hintGroup, 0f);
        }

        // Use hint — F action (only when item selected AND hovering interactable)
        if (_target != null && selected != null)
        {
            if (useHintText) useHintText.text = $"[F] Применить: {selected.itemName}";
            FadeTo(useHintGroup, 1f);
        }
        else
        {
            FadeTo(useHintGroup, 0f);
        }

        // Drop hint — Q action (only when item selected)
        if (selected != null)
        {
            if (dropHintText) dropHintText.text = $"[Q] Выбросить: {selected.itemName}";
            FadeTo(dropHintGroup, 1f);
        }
        else
        {
            FadeTo(dropHintGroup, 0f);
        }
    }

    void FadeTo(CanvasGroup group, float target)
    {
        if (group == null) return;
        group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.deltaTime);
    }

    static void SetAlpha(CanvasGroup group, float a)
    {
        if (group) group.alpha = a;
    }
}
