public interface IInteractable
{
    void Interact(ItemData usedItem);
    string GetHintText(ItemData usedItem);
}
