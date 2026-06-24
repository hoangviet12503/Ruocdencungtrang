using UnityEngine;

namespace MidAutumn.Data
{
    /// <summary>
    /// One stage's win condition: collect targetCount items at targetLevel
    /// simultaneously on the board. Stages escalate target level and/or
    /// count so later stages take meaningfully longer.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelGoal_", menuName = "MidAutumn/Level Goal", order = 4)]
    public class LevelGoalData : ScriptableObject
    {
        public int stageNumber = 1;
        [Range(1, 7)] public int targetLevel = 3;
        [Tooltip("How many items at targetLevel must exist on the board at once to clear the stage.")]
        public int targetCount = 3;
        [Tooltip("Tickets granted on stage clear, on top of any per-merge rewards.")]
        public int clearTicketReward = 30;
    }
}
