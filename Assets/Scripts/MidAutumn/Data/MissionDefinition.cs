using UnityEngine;

namespace MidAutumn.Data
{
    public enum MissionCategory
    {
        Retention,          // Daily check-in, login streak, invite friends
        EcosystemDiscovery, // Tier 1: easy deep-link visits, reward on return
        EcosystemEngagement // Tier 2: hard actions, requires server verification
    }

    public enum MissionRewardType
    {
        Tickets,
    }

    /// <summary>
    /// One row in the Mission screen (Tab 4). Each mission is data, not code,
    /// so Product can add/retune missions across the 60-day campaign without
    /// touching MissionManager.
    /// </summary>
    [CreateAssetMenu(fileName = "Mission_", menuName = "MidAutumn/Mission Definition", order = 2)]
    public class MissionDefinition : ScriptableObject
    {
        [Tooltip("Stable unique id, used as the save-data key. Do not rename after launch.")]
        public string id;

        public string title = "Explore Than Tai Wallet";
        [TextArea] public string description;

        public MissionCategory category = MissionCategory.Retention;
        public MissionRewardType rewardType = MissionRewardType.Tickets;
        public int rewardAmount = 15;

        [Header("Deep Link (Tier 1 Discovery only)")]
        [Tooltip("Internal MoMo deep link, e.g. momo://wallet/than-tai. Passed to the native bridge.")]
        public string deepLink;

        [Header("Server Verification (Tier 2 Engagement only)")]
        [Tooltip("Key the backend uses to identify which server-side action satisfies this mission.")]
        public string serverVerificationKey;

        [Header("Repeatability")]
        [Tooltip("Daily check-in / streak missions reset each day; most ecosystem missions are once-per-campaign.")]
        public bool isDailyRepeating;
    }
}
