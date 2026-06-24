using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MidAutumn.Data;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// Visual + drag/drop input for one cell of the 4x4 board. Drag detection
    /// and merge-eligibility checks happen here; the actual state mutation
    /// is delegated to GameManager.TryMerge so this stays a thin input layer.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GridSlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Canvas rootCanvas;

        private RectTransform _iconRect;
        private Vector2 _iconOriginalAnchoredPos;
        private Transform _iconOriginalParent;
        private int _iconOriginalSiblingIndex;
        private ItemData _currentItem;
        private Tween _breatheTween;

        public int SlotIndex => slotIndex;

        private void Awake()
        {
            _iconRect = itemIcon.rectTransform;
        }

        private void OnEnable()
        {
            GameManager.Instance.OnSlotChanged += HandleSlotChanged;
            GameManager.Instance.OnItemSpawned += HandleItemSpawned;
            Refresh(GameManager.Instance.Slots[slotIndex].item);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSlotChanged -= HandleSlotChanged;
                GameManager.Instance.OnItemSpawned -= HandleItemSpawned;
            }
            _breatheTween?.Kill();
        }

        private void HandleSlotChanged(int changedIndex, ItemData newItem)
        {
            if (changedIndex == slotIndex)
                Refresh(newItem);
        }

        private bool _justSpawned;
        private void HandleItemSpawned(int spawnedIndex)
        {
            if (spawnedIndex == slotIndex) _justSpawned = true;
        }

        private void Refresh(ItemData item)
        {
            _currentItem = item;
            _breatheTween?.Kill();
            _iconRect.localScale = Vector3.one;

            if (item == null)
            {
                itemIcon.enabled = false;
                return;
            }

            itemIcon.enabled = true;
            itemIcon.sprite = item.icon;

            if (_justSpawned)
            {
                _justSpawned = false;
                JuiceFx.PlaySpawnIn(_iconRect);
            }

            JuiceFx.PlayBreathing(_iconRect, ref _breatheTween);
        }

        // ---------- Drag & Drop ----------

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentItem == null) { eventData.pointerDrag = null; return; }

            _breatheTween?.Kill();
            _iconRect.localScale = Vector3.one;

            _iconOriginalAnchoredPos = _iconRect.anchoredPosition;
            _iconOriginalParent = _iconRect.parent;
            _iconOriginalSiblingIndex = _iconRect.GetSiblingIndex();

            // Only the icon (a child of the slot) is reparented to the canvas root so it
            // renders above other slots while dragging. The slot itself (this GameObject)
            // never leaves GridContainer, so its GridLayoutGroup never re-flows siblings.
            _iconRect.SetParent(rootCanvas.transform, true);
            _iconRect.SetAsLastSibling();

            // The icon follows the pointer every frame, so it would otherwise raycast-hit
            // itself and mask whatever slot is actually underneath the cursor.
            itemIcon.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_currentItem == null) return;
            _iconRect.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_currentItem == null) return;

            GridSlotView targetSlot = ResolveDropTarget(eventData);

            itemIcon.raycastTarget = true;

            // Snap back to original parent/position; GameManager events will
            // refresh visuals on both slots if a merge actually happened.
            _iconRect.SetParent(_iconOriginalParent, true);
            _iconRect.SetSiblingIndex(_iconOriginalSiblingIndex);
            _iconRect.anchoredPosition = _iconOriginalAnchoredPos;

            if (targetSlot != null && targetSlot != this)
            {
                bool merged = GameManager.Instance.TryMerge(slotIndex, targetSlot.SlotIndex);
                if (!merged)
                {
                    // Same item snaps back with a small "rejected" shake instead of a silent no-op.
                    JuiceFx.PlayRejectShake(_iconRect);
                }
            }
            else
            {
                // Dropped on empty space / itself: just resume breathing.
                JuiceFx.PlayBreathing(_iconRect, ref _breatheTween);
            }
        }

        private GridSlotView ResolveDropTarget(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var view = result.gameObject.GetComponentInParent<GridSlotView>();
                if (view != null) return view;
            }
            return null;
        }
    }
}
