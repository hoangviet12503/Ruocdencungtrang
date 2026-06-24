using UnityEngine;
using UnityEngine.UI;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// Subscribes to GameManager.OnMergeSuccess and plays the punch + radial
    /// glow on whichever slot view was the merge target. One instance per
    /// Play Game screen; holds the board's GridSlotView/glow Image lookups.
    /// </summary>
    public class MergeFxController : MonoBehaviour
    {
        [Tooltip("Indexed 0-15, same order as GameManager.Slots.")]
        [SerializeField] private GridSlotView[] slotViews;
        [Tooltip("Indexed 0-15. A full-bleed radial glow Image per slot, initially inactive.")]
        [SerializeField] private Image[] glowOverlays;

        private void OnEnable()
        {
            GameManager.Instance.OnMergeSuccess += HandleMergeSuccess;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnMergeSuccess -= HandleMergeSuccess;
        }

        private void HandleMergeSuccess(int sourceIndex, int targetIndex, Data.ItemData resultItem)
        {
            if (targetIndex < 0 || targetIndex >= slotViews.Length) return;

            RectTransform targetRect = (RectTransform)slotViews[targetIndex].transform;
            Image glow = (targetIndex < glowOverlays.Length) ? glowOverlays[targetIndex] : null;

            JuiceFx.PlayMergeSuccess(targetRect, glow, resultItem);
        }
    }
}
