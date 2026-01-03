public interface IMiniGameScoring
{
    void OnObjectDropped(SlotDraggable slot, ItemsSO[] items);
    void OnItemRemovedFromSlot();
}