using UnityEngine;

namespace MidAutumn.Data
{
    /// <summary>
    /// Display info for one stop on the "Đường Đua" map, index-aligned with
    /// GameManager.levelGoals / CurrentStageIndex. Position lives on the scene's
    /// stop RectTransform, not here, to avoid duplicating layout data.
    /// </summary>
    [CreateAssetMenu(fileName = "Province_", menuName = "MidAutumn/Province", order = 5)]
    public class ProvinceData : ScriptableObject
    {
        public int stageIndex;
        public int displayNumber;
        public string displayName;
    }
}
