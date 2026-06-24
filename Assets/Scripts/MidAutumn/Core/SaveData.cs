using System;
using System.Collections.Generic;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// Plain-data snapshot of everything that must survive an app restart.
    /// Serialized with JsonUtility — every field here must stay Json-friendly
    /// (no ScriptableObject refs; items are stored by level int + looked up
    /// against the GameManager's chain root at load time).
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int tickets;
        public int lanternPoints;

        // Board: parallel array to GameManager.Slots, 0 = empty, else item level 1-7.
        public int[] slotLevels = new int[GameManager.GridSize];

        // Campaign timing, anchors the 60-day progression and daily check-in.
        public string campaignStartDateIso;
        public string lastCheckInDateIso;
        public int loginStreak;

        // Mission completion state, keyed by MissionDefinition.id.
        public List<string> completedMissionIds = new List<string>();
        public List<string> claimedMissionIds = new List<string>();

        // Index into GameManager's levelGoals array; the merge-grid stage the player is clearing.
        public int currentStageIndex;

        // Parallel to levelGoals: true once a stage has been cleared at least once via
        // TryCollectStage. Used by the map to show a checkmark; not tied to CurrentStageIndex
        // since JumpToStage lets the player freely revisit any stage out of order.
        public bool[] clearedStages = new bool[GameManager.StageCount];

        // VoucherData.id of every voucher won via the 5% box-open roll (RollVoucherReward).
        // Backs the "Quà tặng của bạn" tab; a voucher can appear more than once if won twice.
        public List<string> wonVoucherIds = new List<string>();

        public static SaveData CreateNew()
        {
            return new SaveData
            {
                tickets = 300, // prototype: generous ticket count so testers aren't blocked
                lanternPoints = 0,
                slotLevels = new int[GameManager.GridSize],
                campaignStartDateIso = DateTime.UtcNow.Date.ToString("o"),
                lastCheckInDateIso = string.Empty,
                loginStreak = 0,
                completedMissionIds = new List<string>(),
                claimedMissionIds = new List<string>(),
                currentStageIndex = 0,
                clearedStages = new bool[GameManager.StageCount],
                wonVoucherIds = new List<string>()
            };
        }
    }
}
