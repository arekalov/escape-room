using UnityEngine;
using UnityEngine.InputSystem;

public class VRPlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform rightControllerTransform;
    public InputActionAsset inputActions;

    [Header("Interaction")]
    [Tooltip("Дальность луча для подбора предметов (нет предмета в руке)")]
    public float interactRange = 3.5f;
    [Tooltip("Радиус сферы для применения предмета (предмет выбран в инвентаре)")]
    public float proximityRange = 0.5f;

    [Header("Hints (GameCanvas)")]
    public CanvasGroup hintGroup;
    public UnityEngine.UI.Text hintText;
    public CanvasGroup dropHintGroup;
    public UnityEngine.UI.Text dropHintText;
    public CanvasGroup useHintGroup;
    public UnityEngine.UI.Text useHintText;

    [Header("Animation")]
    public float fadeSpeed = 10f;

    IInteractable   _target;
    bool            _targetNeedsItem; // true = нужен предмет, false = просто подобрать

    InputAction _triggerAction;
    InputAction _gripAction;
    InputAction _secondaryAction;
    InputAction _inventoryAction; // кнопка A правого контроллера

    void Awake()
    {
        SetAlpha(hintGroup,    0f);
        SetAlpha(dropHintGroup, 0f);
        SetAlpha(useHintGroup,  0f);

        if (inputActions != null)
        {
            _triggerAction   = inputActions["XRI Right Interaction/Activate"];
            _gripAction      = inputActions["XRI Right Interaction/Select"];
            _inventoryAction = inputActions.FindAction("XRI Right Locomotion/Jump", true);
        }
        // B button (SecondaryButton) не в стандартных action maps — создаём напрямую
        _secondaryAction = new UnityEngine.InputSystem.InputAction(
            "BButton",
            UnityEngine.InputSystem.InputActionType.Button,
            "<XRController>{RightHand}/{SecondaryButton}");
    }

    void OnEnable()
    {
        _triggerAction?.Enable();
        _gripAction?.Enable();
        _secondaryAction?.Enable();
        _inventoryAction?.Enable();
    }

    void OnDisable()
    {
        _triggerAction?.Disable();
        _gripAction?.Disable();
        _secondaryAction?.Disable();
        _inventoryAction?.Disable();
    }

    void Update()
    {
        Scan();
        HandleInput();
        HandleInventoryInput();
        UpdateHints();
    }

    void Scan()
    {
        _target = null;
        _targetNeedsItem = false;

        var origin = rightControllerTransform ? rightControllerTransform : transform;
        var selected = InventoryManager.Instance?.GetSelected();

        // Предмет выбран → сначала OverlapSphere рядом, потом луч вперёд
        if (selected != null)
        {
            // 1. Проксимити: контроллер рядом с объектом
            var cols = Physics.OverlapSphere(origin.position, proximityRange, Physics.DefaultRaycastLayers);
            System.Array.Sort(cols, (a, b) =>
                Vector3.SqrMagnitude(a.transform.position - origin.position)
                .CompareTo(Vector3.SqrMagnitude(b.transform.position - origin.position)));

            foreach (var col in cols)
            {
                var interactable = col.GetComponentInParent<IInteractable>();
                if (interactable == null) continue;
                if (!string.IsNullOrEmpty(interactable.GetHintText(selected)))
                {
                    _target = interactable;
                    _targetNeedsItem = true;
                    return;
                }
            }

            // 2. Луч: наводим контроллер на объект издали
            var rayHits = Physics.SphereCastAll(origin.position, 0.06f, origin.forward, interactRange,
                                                Physics.DefaultRaycastLayers);
            System.Array.Sort(rayHits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in rayHits)
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable == null) continue;
                if (!string.IsNullOrEmpty(interactable.GetHintText(selected)))
                {
                    _target = interactable;
                    _targetNeedsItem = true;
                    return;
                }
            }
            return;
        }

        // Предмет не выбран → SphereCast лучом вперёд: подбор с расстояния
        var hits = Physics.SphereCastAll(origin.position, 0.06f, origin.forward, interactRange,
                                         Physics.DefaultRaycastLayers);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;
            if (!string.IsNullOrEmpty(interactable.GetHintText(null)))
            {
                _target = interactable;
                _targetNeedsItem = false;
                return;
            }
        }
    }

    void HandleInventoryInput()
    {
        if (_inventoryAction == null || !_inventoryAction.WasPressedThisFrame()) return;

        var mgr = InventoryManager.Instance;
        if (mgr == null) return;

        int count = mgr.GetItems().Count;
        if (count == 0) { mgr.SelectSlot(-1); return; }

        // Циклично следующий слот; повторное нажатие на последний → снять выбор
        int next = (mgr.SelectedSlot + 1) % count;
        mgr.SelectSlot(next == 0 && mgr.SelectedSlot == count - 1 ? -1 : next);
    }

    void HandleInput()
    {
        var mgr = InventoryManager.Instance;

        // Trigger над слотом инвентаря → выбросить
        int hovSlot = InventorySlot.HoveredSlot;
        if (_triggerAction != null && _triggerAction.WasPressedThisFrame() && hovSlot >= 0)
        {
            var items = mgr?.GetItems();
            if (items != null && hovSlot < items.Count)
            {
                var dropPos  = transform.position + transform.forward * 0.6f;
                var throwVel = transform.forward * 1.8f + Vector3.up * 0.4f;
                mgr.DropItem(hovSlot, dropPos, throwVel);
                AudioManager.PlayThrow();
                return;
            }
        }

        if (_triggerAction != null && _triggerAction.WasPressedThisFrame() && _target != null)
        {
            if (_targetNeedsItem && mgr?.GetSelected() != null)
            {
                // Применить выбранный предмет к цели
                _target.Interact(mgr.GetSelected());
                AudioManager.PlayThrow();
            }
            else if (!_targetNeedsItem)
            {
                // Подобрать предмет
                _target.Interact(null);
                AudioManager.PlayTake();
            }
        }

        // B / Secondary — выбросить выбранный предмет из инвентаря
        if (_secondaryAction != null && _secondaryAction.WasPressedThisFrame()
            && mgr != null && mgr.SelectedSlot >= 0)
        {
            var dropPos  = transform.position + transform.forward * 0.6f;
            var throwVel = transform.forward * 1.8f + Vector3.up * 0.4f;
            mgr.DropItem(mgr.SelectedSlot, dropPos, throwVel);
            AudioManager.PlayThrow();
        }
    }

    void UpdateHints()
    {
        var mgr      = InventoryManager.Instance;
        var selected = mgr?.GetSelected();

        // Hover над слотом инвентаря
        int hovSlot = InventorySlot.HoveredSlot;
        if (hovSlot >= 0)
        {
            var items = mgr?.GetItems();
            if (items != null && hovSlot < items.Count)
            {
                if (hintText) hintText.text = "[Trigger] Выбросить: " + items[hovSlot].itemName;
                FadeTo(hintGroup,    1f);
                FadeTo(useHintGroup, 0f);
                FadeTo(dropHintGroup, 0f);
                return;
            }
        }

        // Подсказка для взаимодействия с объектом
        if (_target != null)
        {
            if (_targetNeedsItem && selected != null)
            {
                var raw = _target.GetHintText(selected);
                if (hintText) hintText.text = raw.Replace("[E]", "[Trigger]").Replace("[F]", "[Trigger]");
                FadeTo(hintGroup, 1f);
            }
            else if (!_targetNeedsItem)
            {
                var raw = _target.GetHintText(null);
                if (hintText) hintText.text = raw.Replace("[E]", "[Trigger]").Replace("[F]", "[Trigger]");
                FadeTo(hintGroup, string.IsNullOrEmpty(raw) ? 0f : 1f);
            }
            else
            {
                FadeTo(hintGroup, 0f);
            }
        }
        else
        {
            FadeTo(hintGroup, 0f);
        }

        // Подсказка "выбранный предмет"
        if (selected != null)
        {
            if (useHintText) useHintText.text = "В руке: " + selected.itemName;
            FadeTo(useHintGroup, 1f);
        }
        else FadeTo(useHintGroup, 0f);

        FadeTo(dropHintGroup, 0f);
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
