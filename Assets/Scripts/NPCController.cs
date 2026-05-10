using System.Collections;
using UnityEngine;

public class NPCController : MonoBehaviour, IInteractable
{
    [Header("Waypoints")]
    public Transform doorExitPoint;

    [Header("Movement")]
    public float walkSpeed = 1.4f;

    [Header("Animator trigger names")]
    public string triggerIdle    = "Idle";
    public string triggerExitRoom = "ExitRoom";

    [Header("Repair durations (seconds)")]
    public float repair1Duration = 4f;
    public float repair2Duration = 4f;

    [Header("Item to accept")]
    public ItemData screwdriverItem;

    public enum NPCState { Repairing1, IdleAtTV, Repairing2, Done }
    public NPCState State { get; private set; } = NPCState.Repairing1;

    Animator _anim;

    void Awake() => _anim = GetComponent<Animator>();

    // ── вызывается GameManager после старта кат-сцены ────────
    public void StartApproachAndFix1()
    {
        StartCoroutine(Repair1Routine());
    }

    // ── вызывается GameManager после передачи отвёртки ───────
    public void StartFix2()
    {
        if (State != NPCState.IdleAtTV) return;
        StartCoroutine(Repair2Routine());
    }

    // ── IInteractable ─────────────────────────────────────────
    public void Interact(ItemData usedItem)
    {
        if (State != NPCState.IdleAtTV) return;
        if (usedItem == null || usedItem != screwdriverItem) return;

        InventoryManager.Instance?.RemoveItem(screwdriverItem);
        PlayAnim(triggerIdle);
        GameManager.Instance?.OnScrewdriverGiven();
    }

    public string GetHintText(ItemData usedItem)
    {
        if (State == NPCState.IdleAtTV && usedItem != null && usedItem == screwdriverItem)
            return "[F] Отдать отвёртку";
        if (State == NPCState.Done) return "";
        return "Не мешай, я работаю";
    }

    // ── ExitRoom (вызывается GameManager в финале) ────────────
    public void ExitRoom()
    {
        gameObject.SetActive(false);
    }

    // ── collision: thrown screwdriver hits NPC ────────────────
    void OnCollisionEnter(Collision collision)
    {
        if (State != NPCState.IdleAtTV) return;
        var sd = collision.collider.GetComponentInParent<ScrewdriverCollectible>();
        if (sd == null) return;

        collision.collider.gameObject.SetActive(false);
        PlayAnim(triggerIdle);
        GameManager.Instance?.OnScrewdriverGiven();
    }

    // ── coroutines ────────────────────────────────────────────
    IEnumerator Repair1Routine()
    {
        State = NPCState.Repairing1;
        // Animator starts in Working state by default — no trigger needed
        yield return new WaitForSeconds(repair1Duration);

        State = NPCState.IdleAtTV;
        GameManager.Instance?.OnNPCRepair1Done();
    }

    IEnumerator Repair2Routine()
    {
        State = NPCState.Repairing2;
        // Keep Idle animation — no anim change
        yield return new WaitForSeconds(repair2Duration);

        State = NPCState.Done;
        GameManager.Instance?.OnNPCRepair2Done();
    }

void PlayAnim(string triggerName)
    {
        if (_anim != null && !string.IsNullOrEmpty(triggerName))
            _anim.SetTrigger(triggerName);
    }
}
