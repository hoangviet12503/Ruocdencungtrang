using UnityEngine;

namespace MidAutumn.Data
{
    /// <summary>
    /// Snapshot of one MoMo friend's map progress, as returned by the social
    /// backend. Plain data — fetched via IMoMoBridge-style social API and
    /// handed to MapController for avatar placement.
    /// </summary>
    [System.Serializable]
    public class FriendProgress
    {
        public string userId;
        public string displayName;
        public Sprite avatar;
        public string avatarUrl; // used when avatar must be downloaded at runtime
        public int stage;        // 0-based index along MapController.pathStops
    }
}
