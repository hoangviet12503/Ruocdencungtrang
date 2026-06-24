using UnityEngine;

namespace MidAutumn.Data
{
    /// <summary>One partner voucher in the Tiệm Quà Trung Thu (Tab 1).</summary>
    [CreateAssetMenu(fileName = "Voucher_", menuName = "MidAutumn/Voucher Data", order = 3)]
    public class VoucherData : ScriptableObject
    {
        public string id;
        public string partnerName;
        public string title;
        public Sprite thumbnail;
        public int costInLanternPoints = 100;
        [Tooltip("Total redemptions available campaign-wide; -1 = unlimited.")]
        public int stockLimit = -1;
    }
}
