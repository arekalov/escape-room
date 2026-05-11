using UnityEngine;

public enum QuestStage
{
    Cutscene,          // Кат-сцена пробуждения
    FreeRoam,          // TV ещё не включён
    NPCRepairing1,     // TV включён, NPC чинит (первый раз)
    CollectItems,      // NPC поднялся — можно исследовать комнату
    GiveScrewdriver,   // Игрок нашёл отвёртку, надо отдать NPC
    NPCRepairing2,     // NPC чинит второй раз
    TVShowsText,       // TV показывает надпись
    Victory            // Дверь открыта
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene references")]
    public Animator      doorAnimator;
    public TVController  tvController;
    public NPCController npcController;

    [Header("Radio subtitles")]
    public RadioSubtitles radio;

    public QuestStage Stage { get; private set; } = QuestStage.Cutscene;

    void Awake() => Instance = this;

    void Start()
    {
        // Кат-сцена заканчивается сама — WakeUpCutscene вызывает OnCutsceneDone
    }

    // --- вызывается WakeUpCutscene по окончании ---
    public void OnCutsceneDone()
    {
        SetStage(QuestStage.NPCRepairing1);
        npcController?.StartApproachAndFix1();
        radio?.PlayLine(RadioLine.Intro);
        AudioManager.StartPhoneMusic(3f);
    }

    // --- NPC закончил первую починку ---
    public void OnNPCRepair1Done()
    {
        SetStage(QuestStage.CollectItems);
    }

    // --- игрок поднял отвёртку (вызывает Collectible через событие InventoryManager) ---
    public void OnScrewdriverPickedUp()
    {
        if (Stage == QuestStage.CollectItems)
            SetStage(QuestStage.GiveScrewdriver);
    }

    // --- NPC принял отвёртку ---
    public void OnScrewdriverGiven()
    {
        SetStage(QuestStage.NPCRepairing2);
        npcController?.StartFix2();
    }

    // --- NPC закончил вторую починку ---
    public void OnNPCRepair2Done()
    {
        SetStage(QuestStage.TVShowsText);
        tvController?.ShowCode();
    }

    // --- игрок ввёл правильный код в замок ---
    public void OnCodeCorrect()
    {
        if (Stage != QuestStage.TVShowsText) return;
        SetStage(QuestStage.Victory);
        doorAnimator?.SetTrigger("Open");
        npcController?.ExitRoom();
        radio?.PlayLine(RadioLine.Win);
        AudioManager.PlayGoodJob();
        AudioManager.PlayMainDoor();
    }

    // --- код отображаемый на TV и принимаемый замком ---
    public const string SecretCode = "1937";

    void SetStage(QuestStage s)
    {
        Stage = s;
        Debug.Log("[Quest] Stage → " + s);
    }
}
