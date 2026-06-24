using UnityEngine;
using MidAutumn.Gameplay;

namespace MidAutumn.UI
{
    /// <summary>Thin relay so the Spawn button's persistent UnityEvent has a concrete instance method to bind to.</summary>
    public class SpawnButtonRelay : MonoBehaviour
    {
        public void SpawnRandomWeighted()
        {
            GameManager.Instance.TrySpawnRandomWeighted(out _);
        }
    }
}
