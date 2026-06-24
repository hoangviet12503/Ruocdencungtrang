using System;
using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;

namespace MidAutumn.UI
{
    /// <summary>
    /// One tappable stop on the "Đường Đua" map: number badge + province name +
    /// optional "cleared" checkmark + "currently playing" highlight ring.
    /// Prototype design: every stop is always unlocked/tappable — tapping any
    /// stop jumps the player straight to that stage (see GameManager.JumpToStage).
    /// </summary>
    public class ProvinceStopView : MonoBehaviour
    {
        [SerializeField] private Text numberLabel;
        [SerializeField] private Text nameLabel;
        [SerializeField] private GameObject checkmarkIcon;
        [SerializeField] private GameObject currentHighlight;
        [SerializeField] private Button tapButton;

        private int _stageIndex;
        private Action<int> _onTapped;

        public void Bind(ProvinceData data, Action<int> onTapped)
        {
            _stageIndex = data.stageIndex;
            _onTapped = onTapped;

            if (numberLabel != null) numberLabel.text = data.displayNumber.ToString();
            if (nameLabel != null) nameLabel.text = data.displayName;
        }

        public void SetState(bool isCurrent, bool isCleared)
        {
            if (currentHighlight != null) currentHighlight.SetActive(isCurrent);
            if (checkmarkIcon != null) checkmarkIcon.SetActive(isCleared);
        }

        private void OnEnable()
        {
            if (tapButton != null) tapButton.onClick.AddListener(HandleTapped);
        }

        private void OnDisable()
        {
            if (tapButton != null) tapButton.onClick.RemoveListener(HandleTapped);
        }

        private void HandleTapped() => _onTapped?.Invoke(_stageIndex);
    }
}
