using UnityEngine;
using UnityEngine.UI;

namespace MidAutumn.UI
{
    /// <summary>Tab 5: static Terms & Conditions panel. Text is content, not code — set via inspector or CMS pull.</summary>
    public class RulesScreen : MonoBehaviour
    {
        [SerializeField] private Text rulesBodyText;
        [TextArea(10, 40)] [SerializeField] private string defaultRulesText;

        private void OnEnable()
        {
            if (rulesBodyText != null && string.IsNullOrEmpty(rulesBodyText.text))
                rulesBodyText.text = defaultRulesText;
        }
    }
}
