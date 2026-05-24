using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;

[RequireComponent(typeof(XRRayInteractor))]
public class VRPlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform rightControllerTransform;
    public InputActionAsset inputActions; // XRI Default Input Actions

    [Header("Interaction")]
    public float interactRange = 3.5f;

    [Header("Hints (GameCanvas)")]
    public CanvasGroup hintGroup;
    public UnityEngine.UI.Text hintText;
    public CanvasGroup dropHintGroup;
    public UnityEngine.UI.Text dropHintText;
    public CanvasGroup useHintGroup;
    public UnityEngine.UI.Text useHintText;

    [Header("Animation")]
    public float fadeSpeed = 10f;

    XRRayInteractor _rayInteractor;
    IInteractable   _target;
    int             _mask;

    InputAction _triggerAction;   // XRI Right Interaction / Activate
    InputAction _gripAction;      // XRI Right Interaction / Select
    InputAction _secondaryAction; // XRI Right Locomotion / Teleport Mode Cancel (B button)

    void Awake()
    {
        _rayInteractor = GetComponent<XRRayInteractor>();
        _mask = ~(1 << gameObject.layer);
        SetAlpha(hintGroup,    0f);
        SetAlpha(dropHintGroup, 0f);
        SetAlpha(useHintGroup,  0f);

        if (inputActions != null)
        {
            _triggerAction   = inputActions["XRI Right Interaction/Activate"];
            _gripAction      = inputActions["XRI Right Interaction/Select"];
            _secondaryAction = inputActions["XRI Right Locomotion/Teleport Mode Cancel"];
        }
    }

    void OnEnable()
    {
        _triggerAction?.Enable();
        _gripAction?.Enable();
        _secondaryAction?.Enable();
    }

    void OnDisable()
    {
        _triggerAction?.Disable();
        _gripAction?.Disable();
        _secondaryAction?.Disable();
    }

    void Update()
    {
        Scan();
        HandleInput();
        UpdateHints();
    }

    void Scan()
    {
        _target = null;
        var origin = rightControllerTransform ? rightControllerTransform : transform;
        var hits = Physics.SphereCastAll(origin.position, 0.06f, origin.forward, interactRange, _mask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        var selected = InventoryManager.Instance?.GetSelected();
        foreach (var hit in hits)
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.GetHintText(selected) != "")
            { _target = interactable; break; }
        }
    }

    void HandleInput()
    {
        var mgr = InventoryManager.Instance;

        if (_triggerAction != null && _triggerAction.WasPressedThisFrame() && _target != null)
        {
            _target.Interact(null);
            AudioManager.PlayTake();
        }

        if (_gripAction != null && _gripAction.WasPressedThisFrame()
            && _target != null && mgr?.GetSelected() != null)
        {
            AudioManager.PlayThrow();
            _target.Interact(mgr.GetSelected());
        }

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

        if (_target != null)
        {
            var eHint = _target.GetHintText(null);
            if (hintText) hintText.text = string.IsNullOrEmpty(eHint) ? "" : eHint.Replace("[E]", "[Trigger]").Replace("[F]", "[Grip]");
            FadeTo(hintGroup, string.IsNullOrEmpty(eHint) ? 0f : 1f);
        }
        else FadeTo(hintGroup, 0f);

        if (_target != null && selected != null)
        {
            if (useHintText) useHintText.text = "[Grip] Применить: " + selected.itemName;
            FadeTo(useHintGroup, 1f);
        }
        else FadeTo(useHintGroup, 0f);

        if (selected != null)
        {
            if (dropHintText) dropHintText.text = "[B] Выбросить: " + selected.itemName;
            FadeTo(dropHintGroup, 1f);
        }
        else FadeTo(dropHintGroup, 0f);
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
