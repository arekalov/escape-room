using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform tvPosition;
    public Transform idlePosition;

    [Header("Settings")]
    public float walkSpeed = 1.5f;
    public float repairDuration = 3f;

    private Animator _animator;
    private bool _busy;

    static readonly int HashWalk   = Animator.StringToHash("Walk");
    static readonly int HashFix    = Animator.StringToHash("Fix");
    static readonly int HashIdle   = Animator.StringToHash("Idle");

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Вызывается GameManager / InteractionSystem когда игрок активировал TV
    public void StartRepair()
    {
        if (_busy) return;
        StartCoroutine(RepairRoutine());
    }

    // Вызывается когда игрок отдал отвёртку
    public void FinishRepair()
    {
        if (_busy) return;
        StartCoroutine(FinishRepairRoutine());
    }

    IEnumerator RepairRoutine()
    {
        _busy = true;
        yield return WalkTo(tvPosition);
        _animator.SetTrigger(HashFix);
        yield return new WaitForSeconds(repairDuration);
        _animator.SetTrigger(HashIdle);
        yield return WalkTo(idlePosition);
        _busy = false;
    }

    IEnumerator FinishRepairRoutine()
    {
        _busy = true;
        yield return WalkTo(tvPosition);
        _animator.SetTrigger(HashFix);
        yield return new WaitForSeconds(repairDuration);
        _animator.SetTrigger(HashIdle);
        GameManager.Instance?.OnNPCFinishedRepair();
        _busy = false;
    }

    IEnumerator WalkTo(Transform target)
    {
        if (target == null) yield break;
        _animator.SetTrigger(HashWalk);
        while (Vector3.Distance(transform.position, target.position) > 0.15f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target.position, walkSpeed * Time.deltaTime);
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            yield return null;
        }
        transform.position = target.position;
        _animator.SetTrigger(HashIdle);
    }
}
