using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;

namespace MidAutumn.UI
{
    /// <summary>
    /// One row in the Tab 4 ScrollRect list. Button label/state changes based
    /// on mission category and completion: "Thực hiện" (do it) -> "Nhận" (claim,
    /// for cases needing a manual claim tap) -> "Đã nhận" (claimed, disabled).
    /// This implementation auto-claims on completion (see MissionManager), so
    /// claimed is the terminal disabled state.
    /// </summary>
    public class MissionRowView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text rewardText;
        [SerializeField] private Button actionButton;
        [SerializeField] private Text actionButtonLabel;
        [SerializeField] private GameObject claimedCheckmark;

        private MissionDefinition _mission;
        private Gameplay.MissionManager _missionManager;

        public void Bind(MissionDefinition mission, Gameplay.MissionManager missionManager)
        {
            _mission = mission;
            _missionManager = missionManager;

            titleText.text = mission.title;
            descriptionText.text = mission.description;
            rewardText.text = mission.rewardType == MissionRewardType.Tickets
                ? $"+{mission.rewardAmount} Kẹo"
                : $"+{mission.rewardAmount} Điểm Lồng Đèn";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(HandleClick);

            Refresh();
        }

        private void Refresh()
        {
            bool claimed = _missionManager.IsClaimed(_mission);
            actionButton.interactable = !claimed;
            claimedCheckmark.SetActive(claimed);
            actionButtonLabel.text = claimed ? "Đã nhận" : ButtonLabelFor(_mission.category);
        }

        private static string ButtonLabelFor(MissionCategory category)
        {
            switch (category)
            {
                case MissionCategory.Retention: return "Điểm Danh";
                case MissionCategory.EcosystemDiscovery: return "Khám Phá";
                case MissionCategory.EcosystemEngagement: return "Thực Hiện";
                default: return "Thực Hiện";
            }
        }

        private void HandleClick()
        {
            // Small punch for tactile feedback on tap, independent of merge-grid juice.
            actionButton.transform.DOComplete();
            actionButton.transform.DOPunchScale(Vector3.one * 0.08f, 0.25f, 6, 0.7f);

            switch (_mission.category)
            {
                case MissionCategory.Retention:
                    _missionManager.DoDailyCheckIn();
                    break;
                case MissionCategory.EcosystemDiscovery:
                    _missionManager.StartDiscoveryMission(_mission);
                    break;
                case MissionCategory.EcosystemEngagement:
                    _missionManager.StartEngagementMission(_mission);
                    break;
            }
        }

        private void OnEnable()
        {
            if (_missionManager != null)
            {
                _missionManager.OnMissionClaimed += HandleAnyMissionClaimed;
                _missionManager.OnMissionFailed += HandleMissionFailed;
            }
        }

        private void OnDisable()
        {
            if (_missionManager != null)
            {
                _missionManager.OnMissionClaimed -= HandleAnyMissionClaimed;
                _missionManager.OnMissionFailed -= HandleMissionFailed;
            }
        }

        private void HandleAnyMissionClaimed(MissionDefinition mission)
        {
            if (mission == _mission) Refresh();
        }

        private void HandleMissionFailed(MissionDefinition mission, string reason)
        {
            if (mission != _mission) return;
            Debug.Log($"[MissionRowView] {mission.id} failed: {reason}");
            // Hook up a toast/snackbar here; left as an integration point for the UI team.
        }
    }
}
