using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;
using MidAutumn.Gameplay;

namespace MidAutumn.UI
{
    /// <summary>
    /// Tab 1: "Quà Tặng". Lists vouchers obtainable through gameplay (the box-open
    /// RNG roll in GameManager.RollVoucherReward) — there is no points-shop redeem
    /// flow, winning is automatic while playing.
    /// </summary>
    public enum RewardsSubTab
    {
        Catalog = 0,  // "Danh sách quà tặng" — the prize pool, what's winnable
        MyGifts = 1   // "Quà tặng của bạn" — vouchers already won via the box-open roll
    }

    public class RewardsScreen : MonoBehaviour
    {
        [SerializeField] private VoucherData[] vouchers;
        [SerializeField] private RewardRowView rowPrefab;
        [SerializeField] private Transform listRoot;
        [SerializeField] private GameObject emptyMyGiftsLabel;

        [Header("Sub-tab buttons (visual highlight only, click wiring is separate)")]
        [SerializeField] private Image catalogTabButtonImage;
        [SerializeField] private Image myGiftsTabButtonImage;
        [SerializeField] private Sprite tabActiveSprite;   // Button_Gold
        [SerializeField] private Sprite tabInactiveSprite; // Button_Teal

        private readonly List<RewardRowView> _rows = new List<RewardRowView>();
        private RewardsSubTab _activeSubTab = RewardsSubTab.Catalog;

        private void OnEnable()
        {
            BuildList();
            UpdateSubTabButtonVisuals();
        }

        public void SetSubTab(RewardsSubTab subTab)
        {
            if (_activeSubTab == subTab) return;
            _activeSubTab = subTab;
            BuildList();
            UpdateSubTabButtonVisuals();
        }

        // Parameterless wrappers so the sub-tab buttons can wire SetSubTab as a
        // persistent UnityEvent listener (Unity's UI Inspector can't bind enum params).
        public void ShowCatalogTab() => SetSubTab(RewardsSubTab.Catalog);
        public void ShowMyGiftsTab() => SetSubTab(RewardsSubTab.MyGifts);

        private void UpdateSubTabButtonVisuals()
        {
            if (catalogTabButtonImage == null || myGiftsTabButtonImage == null) return;
            bool catalogActive = _activeSubTab == RewardsSubTab.Catalog;
            catalogTabButtonImage.sprite = catalogActive ? tabActiveSprite : tabInactiveSprite;
            myGiftsTabButtonImage.sprite = catalogActive ? tabInactiveSprite : tabActiveSprite;
        }


        List<string> _listCurrentVoucherIds = new List<string>();
        private void BuildList()
        {
            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Destroy(listRoot.GetChild(i).gameObject);
            _rows.Clear();
            _listCurrentVoucherIds.Clear();

            if (_activeSubTab == RewardsSubTab.Catalog)
            {
                foreach (var voucher in vouchers)
                {
                    var row = Instantiate(rowPrefab, listRoot);
                    row.BindCatalog(voucher);
                    _rows.Add(row);
                }
                if (emptyMyGiftsLabel != null) emptyMyGiftsLabel.SetActive(false);
            }
            else
            {
                // MyGifts: one row per won voucher id, looked up against the same catalog array.
                // A voucher won twice produces two rows (each win is a distinct gift).
                    
                foreach (string wonId in GameManager.Instance.WonVoucherIds)
                {
                    VoucherData data = FindById(wonId);
                    if (data == null || _listCurrentVoucherIds.Contains(data.id)) continue;
                    _listCurrentVoucherIds.Add(data.id);
                    var row = Instantiate(rowPrefab, listRoot);
                    row.BindOwned(data);
                    _rows.Add(row);
                }
                if (emptyMyGiftsLabel != null) emptyMyGiftsLabel.SetActive(_rows.Count == 0);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)listRoot);
        }

        private VoucherData FindById(string id)
        {
            foreach (var voucher in vouchers)
                if (voucher.id == id) return voucher;
            return null;
        }
    }
}
