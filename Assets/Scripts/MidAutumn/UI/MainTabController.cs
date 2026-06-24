using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MidAutumn.UI
{
    public enum MainTab
    {
        Rewards = 0,
        Map = 1,
        PlayGame = 2,
        Missions = 3,
        Rules = 4
    }

    /// <summary>
    /// Bottom navigation bar controller for the 5 screens. Each tab's panel
    /// is a sibling GameObject under panelsRoot; only the active one is enabled,
    /// so OnEnable/OnDisable on each screen script doubles as a cheap
    /// "screen became visible/hidden" lifecycle hook (see RewardsScreen,
    /// MissionScreen, MapController all relying on this).
    /// Transitions fade+scale the outgoing panel down before disabling it,
    /// then fade+scale the incoming panel up — shared juice across all 5 tabs.
    /// </summary>
    public class MainTabController : MonoBehaviour
    {
        [SerializeField] private Button[] tabButtons;   // length 5, indexed by MainTab
        [SerializeField] private GameObject[] tabPanels; // length 5, indexed by MainTab
        [SerializeField] private MainTab startingTab = MainTab.PlayGame;

        private const float TransitionDuration = 0.18f;
        private CanvasGroup[] _panelCanvasGroups;
        private MainTab _activeTab;
        private bool _isTransitioning;
        private Sequence _activeSequence;

        private void Awake()
        {
            _panelCanvasGroups = new CanvasGroup[tabPanels.Length];
            for (int i = 0; i < tabPanels.Length; i++)
            {
                var cg = tabPanels[i].GetComponent<CanvasGroup>();
                if (cg == null) cg = tabPanels[i].AddComponent<CanvasGroup>();
                _panelCanvasGroups[i] = cg;
            }

            for (int i = 0; i < tabButtons.Length; i++)
            {
                int capturedIndex = i; // avoid closure-over-loop-variable bug
                tabButtons[i].onClick.AddListener(() => SetActiveTab((MainTab)capturedIndex));
            }
        }

        private void Start()
        {
            // First tab shown has no outgoing panel to animate; snap it straight to visible.
            _activeTab = startingTab;
            for (int i = 0; i < tabPanels.Length; i++)
            {
                bool isStarting = i == (int)startingTab;
                tabPanels[i].SetActive(isStarting);
                _panelCanvasGroups[i].alpha = isStarting ? 1f : 0f;
            }
            UpdateButtonInteractable();
        }

        public void SetActiveTab(MainTab tab)
        {
            if (tab == _activeTab) return;
            if (_isTransitioning) return;

            _activeSequence?.Kill();

            int outgoingIndex = (int)_activeTab;
            int incomingIndex = (int)tab;
            _activeTab = tab;

            Debug.Log($"[MainTab] Switching from {(MainTab)outgoingIndex} to {tab}");

            GameObject outgoing = tabPanels[outgoingIndex];
            GameObject incoming = tabPanels[incomingIndex];
            CanvasGroup outgoingCg = _panelCanvasGroups[outgoingIndex];
            CanvasGroup incomingCg = _panelCanvasGroups[incomingIndex];

            outgoingCg.alpha = 0f;
            outgoing.SetActive(false);

            incomingCg.alpha = 1f;
            incoming.SetActive(true);

            Debug.Log($"[MainTab] Instant switch to {incoming.name}, alpha={incomingCg.alpha}");

            UpdateButtonInteractable();
        }

        private void UpdateButtonInteractable()
        {
            for (int i = 0; i < tabButtons.Length; i++)
                tabButtons[i].interactable = i != (int)_activeTab;
        }
    }
}
