using UnityEngine;

namespace MidAutumn.Data
{
    /// <summary>
    /// Defines one rung of the 7-level merge chain (Candle -> Lucky Lion Head).
    /// One asset per level, linked via nextLevelData to form the chain.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData_Lvl", menuName = "MidAutumn/Item Data", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("1-7. Level 7 (Đầu Lân May Mắn) is MAX and cannot merge further.")]
        [Range(1, 7)] public int level = 1;
        public string displayName = "Nến Đêm Rằm";

        [Header("Visuals")]
        public Sprite icon;
        [Tooltip("Tint used for the radial glow VFX when this level is the merge RESULT.")]
        public Color glowColor = new Color(1f, 0.85f, 0.4f);

        [Header("Chain")]
        [Tooltip("Null for level 7 (max level). Assign in inspector to form the chain.")]
        public ItemData nextLevelData;

        public bool IsMaxLevel => nextLevelData == null;
    }
}
