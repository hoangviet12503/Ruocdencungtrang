using System;
using UnityEngine;

namespace MidAutumn.Bridge
{
    /// <summary>
    /// Stub bridge for Editor/standalone testing, where no real MoMo host app
    /// or backend exists. OpenDeepLink fires onReturn after a short delay to
    /// simulate the user alt-tabbing back; CheckServerVerification always
    /// approves so Tier 2 missions are testable end-to-end without a server.
    /// </summary>
    public class EditorMockMoMoBridge : MonoBehaviour, IMoMoBridge
    {
        [SerializeField] private float simulatedReturnDelay = 1.0f;
        [SerializeField] private bool simulatedVerificationResult = true;

        public void OpenDeepLink(string deepLink, Action onReturn)
        {
            Debug.Log($"[EditorMockMoMoBridge] Would open deep link: {deepLink}");
            Invoke(nameof(InvokePendingReturn), simulatedReturnDelay);
            _pendingReturn = onReturn;
        }

        public void CheckServerVerification(string verificationKey, Action<bool> onResult)
        {
            Debug.Log($"[EditorMockMoMoBridge] Mock-verifying: {verificationKey} -> {simulatedVerificationResult}");
            onResult?.Invoke(simulatedVerificationResult);
        }

        private Action _pendingReturn;
        private void InvokePendingReturn()
        {
            var callback = _pendingReturn;
            _pendingReturn = null;
            callback?.Invoke();
        }
    }
}
