using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;
using MidAutumn.Gameplay;

namespace MidAutumn.UI
{
    /// <summary>
    /// Top-of-grid HUD for the Play Game tab: shows the current stage's goal
    /// ("Thu thập 3x Đèn Ông Sao") and a fill bar tracking progress toward it.
    /// Also plays a short celebratory popup when GameManager.OnStageCleared fires.
    /// </summary>
    public class StageGoalView : MonoBehaviour
    {
        [SerializeField] private Text goalLabel;
        [SerializeField] private Image goalIcon;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressLabel;
        [SerializeField] private Button collectButton;
        [SerializeField] private GameObject stageClearPopup;
        [SerializeField] private Text stageClearLabel;
        [SerializeField] private Text campaignCompleteLabel;
        [SerializeField] private GameObject voucherWonPopup;
        [SerializeField] private Text voucherWonLabel;
        [SerializeField] private Image voucherWonIcon;

        private void OnEnable()
        {
            GameManager.Instance.OnSlotChanged += HandleSlotChanged;
            GameManager.Instance.OnStageCleared += HandleStageCleared;
            GameManager.Instance.OnCampaignComplete += HandleCampaignComplete;
            GameManager.Instance.OnVoucherWon += HandleVoucherWon;
            if (collectButton != null) collectButton.onClick.AddListener(HandleCollectClicked);
            Refresh();
        }

        private void OnDisable()
        {
            if (collectButton != null) collectButton.onClick.RemoveListener(HandleCollectClicked);
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnSlotChanged -= HandleSlotChanged;
            GameManager.Instance.OnStageCleared -= HandleStageCleared;
            GameManager.Instance.OnCampaignComplete -= HandleCampaignComplete;
            GameManager.Instance.OnVoucherWon -= HandleVoucherWon;
        }

        private void HandleCollectClicked() => GameManager.Instance.TryCollectStage();

        private void HandleSlotChanged(int index, ItemData item) => Refresh();

        private void Refresh()
        {
            var gm = GameManager.Instance;
            LevelGoalData goal = gm.CurrentLevelGoal;

            if (goal == null)
            {
                goalLabel.text = "All stages complete!";
                progressFill.fillAmount = 1f;
                progressLabel.text = string.Empty;
                if (collectButton != null) collectButton.gameObject.SetActive(false);
                return;
            }

            ItemData targetItem = gm.ItemDatabase.GetByLevel(goal.targetLevel);
            int current = Mathf.Min(gm.CountItemsAtLevel(goal.targetLevel), goal.targetCount);

            goalLabel.text = $"Stage {goal.stageNumber}: Collect {goal.targetCount}x {targetItem.displayName}";
            if (goalIcon != null) goalIcon.sprite = targetItem.icon;
            progressLabel.text = $"{current}/{goal.targetCount}";

            float targetFill = (float)current / goal.targetCount;
            progressFill.DOKill();
            progressFill.DOFillAmount(targetFill, 0.25f).SetEase(Ease.OutQuad);

            if (collectButton != null)
            {
                collectButton.gameObject.SetActive(true);
                collectButton.interactable = gm.IsGoalReady;
            }
        }

        private void HandleStageCleared(LevelGoalData clearedGoal)
        {
            if (stageClearPopup == null) return;

            stageClearLabel.text = $"Stage {clearedGoal.stageNumber} Complete!\n+{clearedGoal.clearTicketReward} Plays";
            PlayPopup(stageClearPopup);
        }

        private void HandleCampaignComplete()
        {
            if (campaignCompleteLabel == null) return;
            PlayPopup(campaignCompleteLabel.gameObject);
        }

        private void HandleVoucherWon(VoucherData voucher)
        {
            if (voucherWonPopup == null) return;

            if (voucherWonLabel != null)
                voucherWonLabel.text = $"Congratulations!\nYou just won {voucher.title}\n({voucher.partnerName})";
            if (voucherWonIcon != null)
                voucherWonIcon.sprite = voucher.thumbnail;

            PlayPopup(voucherWonPopup);
        }

        private void PlayPopup(GameObject popup)
        {
            popup.SetActive(true);
            var rect = popup.transform;
            rect.localScale = Vector3.zero;

            var canvasGroup = popup.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = popup.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            var sequence = DOTween.Sequence();
            sequence.Append(rect.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack));
            sequence.Append(rect.DOScale(1f, 0.1f));
            sequence.AppendInterval(1.4f);
            sequence.Append(canvasGroup.DOFade(0f, 0.3f));
            sequence.OnComplete(() => popup.SetActive(false));
        }
    }
}
