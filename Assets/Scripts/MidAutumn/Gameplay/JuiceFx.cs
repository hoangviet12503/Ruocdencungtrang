using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// Centralized DOTween "game feel" helpers for the merge grid: idle
    /// breathing, merge punch + radial glow, and a reject shake for invalid drops.
    /// Kept static/stateless except for the tween handle the caller owns,
    /// so any UI element can reuse the same juice without duplicating tween setup.
    /// </summary>
    public static class JuiceFx
    {
        private const float BreatheScale = 1.06f;
        private const float BreatheDuration = 1.2f;

        /// <summary>Continuous idle float/breathe loop for items sitting on the grid.</summary>
        public static void PlayBreathing(RectTransform target, ref Tween handle)
        {
            handle?.Kill();
            target.localScale = Vector3.one;
            handle = target
                .DOScale(BreatheScale, BreatheDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(target); // lets DOTween.Kill(target) clean up if the object is destroyed mid-tween
        }

        /// <summary>Small negative shake to communicate "this drop was rejected" (mismatched levels, etc).</summary>
        public static void PlayRejectShake(RectTransform target)
        {
            target.DOShakeAnchorPos(0.3f, strength: 12f, vibrato: 20, randomness: 90, fadeOut: true);
        }

        /// <summary>
        /// Pop-in for a freshly spawned item (e.g. from opening the gift box): scales
        /// up from zero with overshoot so new items read as "just arrived" rather than
        /// silently appearing, distinct from the idle breathing loop that follows.
        /// </summary>
        public static void PlaySpawnIn(RectTransform target)
        {
            target.DOComplete();
            target.localScale = Vector3.zero;
            target.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// The signature "merge moment": elastic punch-scale on the result item plus
        /// a radial golden/pink glow burst. Call on the TARGET slot's icon once
        /// GameManager.OnMergeSuccess fires.
        /// </summary>
        public static void PlayMergeSuccess(RectTransform resultTransform, Image glowImage, ItemData resultItem)
        {
            // Elastic punch/bounce on the merged item itself.
            resultTransform.DOComplete();
            resultTransform.localScale = Vector3.one;
            resultTransform
                .DOPunchScale(Vector3.one * 0.45f, duration: 0.5f, vibrato: 8, elasticity: 0.9f)
                .SetEase(Ease.OutElastic);

            // Radial glow burst: scale-from-zero + fade-out, tinted per-level via ItemData.glowColor.
            if (glowImage == null) return;

            glowImage.gameObject.SetActive(true);
            glowImage.color = new Color(resultItem.glowColor.r, resultItem.glowColor.g, resultItem.glowColor.b, 0.9f);
            glowImage.rectTransform.localScale = Vector3.zero;

            Sequence glowSeq = DOTween.Sequence();
            glowSeq.Append(glowImage.rectTransform.DOScale(2.2f, 0.45f).SetEase(Ease.OutCubic));
            glowSeq.Join(glowImage.DOFade(0f, 0.45f).SetEase(Ease.InCubic));
            glowSeq.OnComplete(() => glowImage.gameObject.SetActive(false));
        }
    }
}
