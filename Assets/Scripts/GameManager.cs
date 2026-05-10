using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene references")]
    public Animator doorAnimator;
    public Animator safeAnimator;
    public TVController tvController;

    void Awake()
    {
        Instance = this;
    }

    // Вызывается TVController когда TV выключают
    public void OnTVTurnedOff()
    {
        doorAnimator?.SetTrigger("Open");
    }

    // Вызывается когда NPC закончил ремонт
    public void OnNPCFinishedRepair()
    {
        tvController?.ShowCode();
    }

    // Вызывается когда игрок открывает сейф
    public void OpenSafe()
    {
        safeAnimator?.SetTrigger("Open");
    }

    // Тест-хелперы (убрать перед финальным билдом)
    [ContextMenu("TEST: Open Door")]
    void TestOpenDoor() => doorAnimator?.SetTrigger("Open");

    [ContextMenu("TEST: Open Safe")]
    void TestOpenSafe() => safeAnimator?.SetTrigger("Open");

    [ContextMenu("TEST: TV Show Code")]
    void TestTVCode() => tvController?.ShowCode();
}
