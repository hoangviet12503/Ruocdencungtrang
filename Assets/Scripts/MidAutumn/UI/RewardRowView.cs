using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;

namespace MidAutumn.UI
{
    /// <summary>
    /// One voucher card in the Quà Tặng tab. Vouchers are won automatically through
    /// gameplay (the box-open RNG roll, see GameManager.RollVoucherReward) — there is
    /// no points-shop redeem flow, so this view is display-only in both sub-tabs.
    /// </summary>
    public class RewardRowView : MonoBehaviour
    {
        [SerializeField] private Image thumbnail;
        [SerializeField] private Text partnerNameText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text costText;
        [SerializeField] private Button redeemButton;
        [SerializeField] private Text stockText;
        [SerializeField] private GameObject ownedBadge;

        /// <summary>Catalog row: a prize obtainable through gameplay ("what's in the prize pool").</summary>
        public void BindCatalog(VoucherData voucher)
        {
            thumbnail.sprite = voucher.thumbnail;
            partnerNameText.text = voucher.partnerName;
            titleText.text = voucher.title;
            if (costText != null) costText.text = "Có thể nhận khi chơi";
            if (stockText != null) stockText.text = string.Empty;
            if (ownedBadge != null) ownedBadge.SetActive(false);
            redeemButton.gameObject.SetActive(false);
        }

        /// <summary>"Quà tặng của bạn" row: a voucher already won.</summary>
        public void BindOwned(VoucherData voucher)
        {
            thumbnail.sprite = voucher.thumbnail;
            partnerNameText.text = voucher.partnerName;
            titleText.text = voucher.title;
            if (costText != null) costText.text = string.Empty;
            if (stockText != null) stockText.text = string.Empty;
            if (ownedBadge != null) ownedBadge.SetActive(true);
            redeemButton.gameObject.SetActive(false);
        }
    }
}
