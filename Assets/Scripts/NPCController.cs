using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("Exit target (empty GO near door exit)")]
    public Transform doorExitPoint;

    [Header("Settings")]
    public float walkSpeed = 1.5f;

    Animator _animator;
    static readonly int HashExitRoom = Animator.StringToHash("ExitRoom");

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void ExitRoom()
    {
        StartCoroutine(WalkOutRoutine());
    }

    IEnumerator WalkOutRoutine()
    {
        yield return new UnityEngine.WaitForSeconds(3f);
        _animator.SetTrigger(HashExitRoom);

        if (doorExitPoint == null) yield break;

        while (Vector3.Distance(transform.position, doorExitPoint.position) > 0.2f)
        {
            Vector3 target = new Vector3(doorExitPoint.position.x, transform.position.y, doorExitPoint.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
            transform.LookAt(target);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
