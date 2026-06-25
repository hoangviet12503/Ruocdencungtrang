using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Gameplay;

namespace MidAutumn.UI
{
    /// <summary>
    /// Persistent header showing Tickets, Lantern Points, and days remaining
    /// in the 60-day campaign. Lives above the tab panels, outside MainTabController's
    /// show/hide cycle, since it's visible on every tab.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] private Text ticketsText;
        [SerializeField] private Text lanternPointsText;
        [SerializeField] private Text daysRemainingText;

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            gm.OnTicketsChanged += UpdateTickets;
            gm.OnLanternPointsChanged += UpdatePoints;

            UpdateTickets(gm.Tickets);
            UpdatePoints(gm.LanternPoints);
            daysRemainingText.text = $"{gm.CampaignDaysRemaining} days left";
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnTicketsChanged -= UpdateTickets;
            GameManager.Instance.OnLanternPointsChanged -= UpdatePoints;
        }

        private void UpdateTickets(int value) => ticketsText.text = $"{value} Candy";
        private void UpdatePoints(int value) => lanternPointsText.text = value.ToString();
    }
}
