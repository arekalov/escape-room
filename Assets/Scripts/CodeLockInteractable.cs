using UnityEngine;

public class CodeLockInteractable : MonoBehaviour, IInteractable
{
    public void Interact(ItemData usedItem)
    {
        if (GameManager.Instance?.Stage != QuestStage.TVShowsText) return;
        CodeInputPanel.Instance?.Open();
    }

    public string GetHintText(ItemData usedItem)
    {
        if (GameManager.Instance?.Stage == QuestStage.TVShowsText)
            return "[E] Ввести код";
        return "";
    }
}
