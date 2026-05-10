using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 2.5f;
    public Transform cam;

    [Header("Hint UI")]
    public CanvasGroup hintGroup;
    public Text        hintText;

    [Header("Hint Animation")]
    public float fadeSpeed = 8f;

    int          _mask;
    IInteractable _target;
    float        _targetAlpha;
    Coroutine    _fadeRoutine;

    void Awake()
    {
        _mask = ~(1 << gameObject.layer);
        if (hintGroup) hintGroup.alpha = 0f;
    }

    void Update()
    {
        Scan();
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && _target != null)
            _target.Interact(InventoryManager.Instance?.GetSelected());

        // Smooth fade
        if (hintGroup && !Mathf.Approximately(hintGroup.alpha, _targetAlpha))
            hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
    }

    void Scan()
    {
        var origin = cam ? cam : Camera.main.transform;
        var hits   = Physics.RaycastAll(origin.position, origin.forward, interactRange, _mask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                _target = interactable;
                var held = InventoryManager.Instance?.GetSelected();
                if (hintText) hintText.text = interactable.GetHintText(held);
                _targetAlpha = 1f;
                return;
            }
        }
        _target      = null;
        _targetAlpha = 0f;
    }
}
