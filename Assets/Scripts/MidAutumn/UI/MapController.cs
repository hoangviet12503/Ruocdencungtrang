using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;
using MidAutumn.Gameplay;

namespace MidAutumn.UI
{
    /// <summary>
    /// Tab 2: "Đường Đua Rước Đèn" — a vertical ScrollRect path made of 15
    /// province stops (one per stage, artist-placed RectTransform markers).
    /// The player's marker sits at GameManager.MapStage, which always mirrors
    /// CurrentStageIndex (no independent counter). Prototype design: every
    /// stop is unlocked and tappable — tapping any province jumps straight to
    /// that stage via GameManager.JumpToStage, then switches to the Play tab.
    /// Friends' avatars are instantiated at their reported stage from the
    /// social API (unrelated to stage progress, untouched by this feature).
    /// </summary>
    public class MapController : MonoBehaviour
    {
        [Tooltip("Ordered bottom-to-top or top-to-bottom anchor points along the path art, one per stage.")]
        [SerializeField] private RectTransform[] pathStops;

        [Tooltip("Index-aligned with pathStops / GameManager.levelGoals, one per stage.")]
        [SerializeField] private ProvinceData[] provinces;

        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private FriendAvatarView friendAvatarPrefab;
        [SerializeField] private Transform friendMarkersRoot;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private MainTabController mainTabController;

        private readonly List<FriendAvatarView> _spawnedFriendMarkers = new List<FriendAvatarView>();
        private ProvinceStopView[] _stopViews;
        private bool _bound;

        private void Awake()
        {
            _stopViews = new ProvinceStopView[pathStops.Length];
            for (int i = 0; i < pathStops.Length; i++)
                _stopViews[i] = pathStops[i].GetComponent<ProvinceStopView>();

            if (pathStops.Length != provinces.Length)
                Debug.LogWarning($"[MapController] pathStops ({pathStops.Length}) and provinces ({provinces.Length}) length mismatch.");
        }

        private void OnEnable()
        {
            BindStopsIfNeeded();
            RefreshAllStops();
            PlacePlayerMarker(GameManager.Instance.MapStage);
            ScrollToStage(GameManager.Instance.MapStage);
            GameManager.Instance.OnStageCleared += HandleStageCleared;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnStageCleared -= HandleStageCleared;
        }

        private void BindStopsIfNeeded()
        {
            if (_bound) return;
            _bound = true;

            for (int i = 0; i < _stopViews.Length && i < provinces.Length; i++)
                _stopViews[i].Bind(provinces[i], HandleStopTapped);
        }

        private void RefreshAllStops()
        {
            int current = GameManager.Instance.MapStage;
            for (int i = 0; i < _stopViews.Length; i++)
                _stopViews[i].SetState(isCurrent: i == current, isCleared: GameManager.Instance.IsStageCleared(i));
        }

        private void HandleStageCleared(LevelGoalData clearedGoal)
        {
            RefreshAllStops();
            int newStage = GameManager.Instance.MapStage;
            PlacePlayerMarker(newStage);
            ScrollToStage(newStage);
        }

        private void HandleStopTapped(int stageIndex)
        {
            GameManager.Instance.JumpToStage(stageIndex);
            RefreshAllStops();
            PlacePlayerMarker(stageIndex);
            mainTabController.SetActiveTab(MainTab.PlayGame);
        }

        /// <summary>Called once friend data arrives from the social backend.</summary>
        public void RenderFriends(IReadOnlyList<FriendProgress> friends)
        {
            ClearFriendMarkers();

            foreach (var friend in friends)
            {
                int stage = Mathf.Clamp(friend.stage, 0, pathStops.Length - 1);
                var marker = Instantiate(friendAvatarPrefab, friendMarkersRoot);
                marker.Bind(friend);
                ((RectTransform)marker.transform).anchoredPosition = pathStops[stage].anchoredPosition;
                _spawnedFriendMarkers.Add(marker);
            }
        }

        private void ClearFriendMarkers()
        {
            foreach (var marker in _spawnedFriendMarkers)
                if (marker != null) Destroy(marker.gameObject);
            _spawnedFriendMarkers.Clear();
        }

        public void PlacePlayerMarker(int stage)
        {
            stage = Mathf.Clamp(stage, 0, pathStops.Length - 1);
            playerMarker.anchoredPosition = pathStops[stage].anchoredPosition;
        }

        public void ScrollToStage(int stage)
        {
            stage = Mathf.Clamp(stage, 0, pathStops.Length - 1);
            // Stops are placed by real map coordinates (not evenly by index), so we
            // scroll based on the stop's actual position within content rather than
            // its index. Content has pivot (0.5, 0): a stop's offset from content's
            // bottom edge is (anchoredPosition.y + content.rect.height / 2).
            RectTransform content = scrollRect.content;
            RectTransform viewport = scrollRect.viewport;
            float viewportH = viewport.rect.height;
            float contentH = content.rect.height;
            float scrollableH = Mathf.Max(0f, contentH - viewportH);

            float stopOffsetFromBottom = pathStops[stage].anchoredPosition.y + contentH * 0.5f;
            float desired = Mathf.Clamp(stopOffsetFromBottom - viewportH * 0.5f, 0f, scrollableH);

            // ScrollRect's Elastic movementType actively corrects content.anchoredPosition
            // in its own LateUpdate if it disagrees with the position; go through its own
            // verticalNormalizedPosition setter (and stop any residual velocity) so the
            // change actually sticks instead of being overwritten next frame.
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = scrollableH <= 0f ? 0f : desired / scrollableH;
        }
    }
}
