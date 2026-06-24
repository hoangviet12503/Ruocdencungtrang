using UnityEngine;

namespace MidAutumn.Data
{
    /// <summary>
    /// Flat lookup table for all 7 ItemData assets, indexed by level.
    /// GameManager walks the nextLevelData chain during gameplay, but save/load
    /// and any UI that needs "give me level N's icon" need random access instead.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "MidAutumn/Item Database", order = 1)]
    public class ItemDatabase : ScriptableObject
    {
        [Tooltip("Must contain exactly 7 entries, index 0 = level 1 ... index 6 = level 7.")]
        [SerializeField] private ItemData[] itemsByLevel = new ItemData[7];

        public ItemData GetByLevel(int level)
        {
            int index = level - 1;
            if (index < 0 || index >= itemsByLevel.Length)
            {
                Debug.LogError($"[ItemDatabase] Level {level} out of range.");
                return null;
            }
            return itemsByLevel[index];
        }

        public int Count => itemsByLevel.Length;
    }
}
