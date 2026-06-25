using System;
using UnityEngine;
using MidAutumn.Bridge;
using MidAutumn.Data;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// The growth engine (Tab 4). Drives the Retention loop (check-in, streak,
    /// invite) and the two-tier Ecosystem Adoption loop:
    ///   Tier 1 Discovery  - deep link out, reward on return, no server call.
    ///   Tier 2 Engagement - reward gated behind IMoMoBridge.CheckServerVerification.
    /// All mission completion/claim state is persisted via GameManager so the
    /// list survives app restarts and daily missions reset correctly.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private MissionDefinition[] missions;
        [SerializeField] private MonoBehaviour bridgeBehaviour; // must implement IMoMoBridge

        private IMoMoBridge _bridge;

        public event Action<MissionDefinition> OnMissionCompleted; // ready to claim
        public event Action<MissionDefinition> OnMissionClaimed;   // reward granted
        public event Action<MissionDefinition, string> OnMissionFailed; // verification rejected etc.

        public MissionDefinition[] Missions => missions;

        private void Awake()
        {
            _bridge = bridgeBehaviour as IMoMoBridge;
            if (_bridge == null)
                Debug.LogError("[MissionManager] bridgeBehaviour does not implement IMoMoBridge.");
        }

        private void OnEnable()
        {
            GameManager.Instance.OnCheckedIn += HandleCheckedIn;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnCheckedIn -= HandleCheckedIn;
        }

        /// <summary>True if the mission's completion gate is satisfied and reward not yet claimed.</summary>
        public bool IsClaimable(MissionDefinition mission)
        {
            return GameManager.Instance.IsMissionCompleted(mission.id)
                && !GameManager.Instance.IsMissionClaimed(mission.id);
        }

        public bool IsClaimed(MissionDefinition mission) => GameManager.Instance.IsMissionClaimed(mission.id);

        // ---------- Retention loop ----------

        /// <summary>Entry point for the "Điểm Danh" (Daily Check-in) button.</summary>
        public void DoDailyCheckIn()
        {
            GameManager.Instance.TryDailyCheckIn(20);
        }

        private void HandleCheckedIn(int newStreak)
        {
            // Streak milestone missions (3-day / 7-day) are plain MissionDefinitions
            // whose completion is driven by GameManager.LoginStreak rather than a
            // button press; check them here whenever the streak advances.
            foreach (var mission in missions)
            {
                if (mission.category != MissionCategory.Retention) continue;
                if (mission.id == "streak_3" && newStreak >= 3) CompleteAndAutoClaim(mission);
                if (mission.id == "streak_7" && newStreak >= 7) CompleteAndAutoClaim(mission);
            }
        }

        /// <summary>Call when the native share sheet reports a successful invite.</summary>
        public void NotifyFriendInvited(MissionDefinition inviteMission)
        {
            CompleteAndAutoClaim(inviteMission);
        }

        // ---------- Tier 1: Discovery (deep link, reward on return) ----------

        public void StartDiscoveryMission(MissionDefinition mission)
        {
            if (mission.category != MissionCategory.EcosystemDiscovery)
            {
                Debug.LogError($"[MissionManager] {mission.id} is not a Discovery mission.");
                return;
            }
            if (IsClaimed(mission)) return;

            _bridge.OpenDeepLink(mission.deepLink, () => CompleteAndAutoClaim(mission));
        }

        // ---------- Tier 2: Engagement (server-verified) ----------

        public void StartEngagementMission(MissionDefinition mission)
        {
            if (mission.category != MissionCategory.EcosystemEngagement)
            {
                Debug.LogError($"[MissionManager] {mission.id} is not an Engagement mission.");
                return;
            }
            if (IsClaimed(mission)) return;

            _bridge.CheckServerVerification(mission.serverVerificationKey, verified =>
            {
                if (verified)
                    CompleteAndAutoClaim(mission);
                else
                    OnMissionFailed?.Invoke(mission, "Not completed yet. Please try again later.");
            });
        }

        // ---------- Shared completion/claim ----------

        private void CompleteAndAutoClaim(MissionDefinition mission)
        {
            if (IsClaimed(mission)) return;

            GameManager.Instance.MarkMissionCompleted(mission.id);
            OnMissionCompleted?.Invoke(mission);

            GrantReward(mission);
            GameManager.Instance.MarkMissionClaimed(mission.id);
            OnMissionClaimed?.Invoke(mission);
        }

        private void GrantReward(MissionDefinition mission)
        {
            switch (mission.rewardType)
            {
                case MissionRewardType.Tickets:
                    GameManager.Instance.AddTickets(mission.rewardAmount);
                    break;
                // case MissionRewardType.LanternPoints:
                //     GameManager.Instance.AddLanternPoints(mission.rewardAmount);
                //     break;
            }
        }
    }
}
