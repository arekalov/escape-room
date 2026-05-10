using System.Collections;
using UnityEngine;

public class NPCController : MonoBehaviour, IInteractable
{
    [Header("Waypoints")]
    public Transform tvWorkPoint;    // позиция у TV для починки
    public Transform doorExitPoint;  // точка выхода

    [Header("Movement")]
    public float walkSpeed = 1.4f;

    [Header("Animator triggers/params")]
    public string triggerWalk     = "Walk";
    public string triggerIdle     = "Idle";
    public string triggerFix      = "Fix";
    public string triggerReceive  = "Receive";
    public string triggerExit     = "Exit";

    [Header("Repair durations (seconds)")]
    public float repair1Duration = 4f;
    public float repair2Duration = 4f;

    [Header("Item to accept")]
    public ItemData screwdriverItem;

    public enum NPCState { Waiting, WalkingToTV, Repairing1, IdleAtTV, Repairing2, Done }
    public NPCState State { get; private set; } = NPCState.Waiting;

    Animator _anim;

    void Awake() => _anim = GetComponent<Animator>();

    // ── вызывается GameManager после включения TV ─────────────
    public void StartApproachAndFix1()
    {
        if (State != NPCState.Waiting) return;
        StartCoroutine(ApproachRoutine());
    }

    // ── вызывается GameManager после передачи отвёртки ────────
    public void StartFix2()
    {
        if (State != NPCState.IdleAtTV) return;
        StartCoroutine(Repair2Routine());
    }

    // ── IInteractable (получить отвёртку) ─────────────────────
    public void Interact(ItemData usedItem)
    {
        if (State != NPCState.IdleAtTV) return;
        if (usedItem == null || usedItem != screwdriverItem) return;

        PlayAnim(triggerReceive);
        InventoryManager.Instance?.RemoveItem(screwdriverItem);
        GameManager.Instance?.OnScrewdriverGiven();
    }

    public string GetHintText(ItemData usedItem)
    {
        if (State == NPCState.IdleAtTV && usedItem != null && usedItem == screwdriverItem)
            return "[F] Отдать отвёртку";
        if (State == NPCState.IdleAtTV)
            return "Мастер ждёт инструмент";
        return "";
    }

    // ── ExitRoom (вызывается GameManager в финале) ────────────
    public void ExitRoom()
    {
        if (doorExitPoint == null) { gameObject.SetActive(false); return; }
        StartCoroutine(ExitRoutine());
    }

    // ── coroutines ────────────────────────────────────────────
    IEnumerator ApproachRoutine()
    {
        State = NPCState.WalkingToTV;
        PlayAnim(triggerWalk);

        yield return WalkTo(tvWorkPoint);

        State = NPCState.Repairing1;
        PlayAnim(triggerFix);
        yield return new WaitForSeconds(repair1Duration);

        PlayAnim(triggerIdle);
        State = NPCState.IdleAtTV;
        GameManager.Instance?.OnNPCRepair1Done();
    }

    IEnumerator Repair2Routine()
    {
        State = NPCState.Repairing2;
        PlayAnim(triggerFix);
        yield return new WaitForSeconds(repair2Duration);

        PlayAnim(triggerIdle);
        State = NPCState.Done;
        GameManager.Instance?.OnNPCRepair2Done();
    }

    IEnumerator ExitRoutine()
    {
        yield return new WaitForSeconds(1f);
        PlayAnim(triggerExit);

        while (Vector3.Distance(transform.position, doorExitPoint.position) > 0.25f)
        {
            var target = new Vector3(doorExitPoint.position.x, transform.position.y, doorExitPoint.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
            transform.LookAt(target);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    IEnumerator WalkTo(Transform dest)
    {
        if (dest == null) yield break;
        while (Vector3.Distance(transform.position, dest.position) > 0.25f)
        {
            var target = new Vector3(dest.position.x, transform.position.y, dest.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
            transform.LookAt(target);
            yield return null;
        }
    }

    void PlayAnim(string triggerName)
    {
        if (_anim != null && !string.IsNullOrEmpty(triggerName))
            _anim.SetTrigger(triggerName);
    }
}
