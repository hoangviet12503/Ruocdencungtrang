using System;

namespace MidAutumn.Bridge
{
    /// <summary>
    /// Abstraction over the native MoMo host-app bridge plugin. The mini-game
    /// runs embedded inside MoMo (WebGL or native view), so navigation to other
    /// app sections and server-verified actions go through this contract rather
    /// than calling a concrete plugin directly — keeps MissionManager testable
    /// and lets us swap in a mock/editor stub when running standalone.
    /// </summary>
    public interface IMoMoBridge
    {
        /// <summary>
        /// Tier 1 (Discovery): opens an internal MoMo deep link (e.g. Ví Thần Tài, Quỹ Nhóm)
        /// and invokes onReturn when the user comes back to the mini-game webview/scene.
        /// Reward is granted client-side immediately on return — no server round trip.
        /// </summary>
        void OpenDeepLink(string deepLink, Action onReturn);

        /// <summary>
        /// Tier 2 (Engagement): asks the backend whether the user has completed the
        /// action tied to verificationKey (e.g. "set_monthly_spend_limit"). Must NOT
        /// grant reward client-side until this returns true — avoids reward farming
        /// since these missions pay out 50 tickets.
        /// </summary>
        void CheckServerVerification(string verificationKey, Action<bool> onResult);
    }
}
