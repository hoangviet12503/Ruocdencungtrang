using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;
using MidAutumn.Gameplay;

namespace MidAutumn.UI
{
    /// <summary>
    /// Tab 4 root: populates the ScrollRect content with one MissionRowView
    /// per MissionDefinition on the MissionManager, grouped under "Nhiệm Vụ
    /// Hàng Ngày" (isDailyRepeating) and "Nhiệm Vụ Chung" (everything else)
    /// section headers, matching the reference layout. Simple instantiate-on-open;
    /// swap for a pooled/virtualized list if the mission count grows large.
    /// </summary>
    public class MissionScreen : MonoBehaviour
    {
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private MissionRowView rowPrefab;
        [SerializeField] private Transform scrollContentRoot;
        [SerializeField] private GameObject sectionHeaderPrefab; // has a single Text child
        [SerializeField] private string dailySectionTitle = "Nhiệm Vụ Hàng Ngày";
        [SerializeField] private string generalSectionTitle = "Nhiệm Vụ Chung";

        private void OnEnable()
        {
            BuildList();
        }

        private void BuildList()
        {
            for (int i = scrollContentRoot.childCount - 1; i >= 0; i--)
                Destroy(scrollContentRoot.GetChild(i).gameObject);

            AddSection(dailySectionTitle, daily: true);
            AddSection(generalSectionTitle, daily: false);
        }

        private void AddSection(string title, bool daily)
        {
            bool any = false;
            foreach (var mission in missionManager.Missions)
            {
                if (mission.isDailyRepeating != daily) continue;
                if (!any) { CreateSectionHeader(title); any = true; }
                var row = Instantiate(rowPrefab, scrollContentRoot);
                row.Bind(mission, missionManager);
            }
        }

        private void CreateSectionHeader(string title)
        {
            if (sectionHeaderPrefab == null) return;
            var header = Instantiate(sectionHeaderPrefab, scrollContentRoot);
            var text = header.GetComponentInChildren<Text>(true);
            if (text != null) text.text = title;
        }
    }
}
