using MidAutumn.Data;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// Plain-data state for one cell of the 4x4 board. No MonoBehaviour —
    /// this is the model; GridSlotView is the corresponding visual.
    /// </summary>
    [System.Serializable]
    public class GridSlot
    {
        public int index;          // 0..15, row-major
        public ItemData item;      // null when empty
        public bool IsEmpty => item == null;

        public GridSlot(int index)
        {
            this.index = index;
            item = null;
        }

        public void SetItem(ItemData newItem) => item = newItem;
        public void Clear() => item = null;
    }
}
