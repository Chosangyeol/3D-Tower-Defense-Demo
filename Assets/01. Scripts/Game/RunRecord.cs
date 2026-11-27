using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 판과 판 사이에 남는 기록: 클리어 횟수, 최고 도달 웨이브, 최고 난이도 클리어, 선택한 제약.
    /// 진행 데이터(에셋 이름)별로 저장한다. 영구 성장은 아직 정해지지 않아 저장하지 않는다.
    /// </summary>
    public static class RunRecord
    {
        public static int ClearCount(CampaignDefinition campaign)
        {
            return PlayerPrefs.GetInt(Key(campaign, "clearCount"), 0);
        }

        public static int BestWave(CampaignDefinition campaign)
        {
            return PlayerPrefs.GetInt(Key(campaign, "bestWave"), 0);
        }

        /// <summary>클리어한 판 중 가장 높은 난이도 점수.</summary>
        public static int BestScore(CampaignDefinition campaign)
        {
            return PlayerPrefs.GetInt(Key(campaign, "bestScore"), 0);
        }

        /// <summary>한 번이라도 클리어해야 제약을 고를 수 있다.</summary>
        public static bool IsConstraintUnlocked(CampaignDefinition campaign)
        {
            return ClearCount(campaign) > 0;
        }

        public static void Register(CampaignDefinition campaign, bool cleared, int reachedWave, int score)
        {
            PlayerPrefs.SetInt(Key(campaign, "bestWave"), Mathf.Max(BestWave(campaign), reachedWave));
            if (cleared)
            {
                PlayerPrefs.SetInt(Key(campaign, "bestScore"), Mathf.Max(BestScore(campaign), score));
                PlayerPrefs.SetInt(Key(campaign, "clearCount"), ClearCount(campaign) + 1);
            }
            PlayerPrefs.Save();
        }

        /// <summary>선택해 둔 제약. 아직 클리어하지 않았으면 비어 있다.</summary>
        public static List<ConstraintDefinition> SelectedConstraints(CampaignDefinition campaign)
        {
            if (!IsConstraintUnlocked(campaign)) return new List<ConstraintDefinition>();

            var names = new HashSet<string>(PlayerPrefs.GetString(Key(campaign, "constraints"), string.Empty).Split('\n'));
            return campaign.Constraints.Where(c => c != null && names.Contains(c.name)).ToList();
        }

        public static void SaveSelection(CampaignDefinition campaign, IEnumerable<ConstraintDefinition> selected)
        {
            PlayerPrefs.SetString(Key(campaign, "constraints"), string.Join("\n", selected.Select(c => c.name)));
            PlayerPrefs.Save();
        }

        public static void Reset(CampaignDefinition campaign)
        {
            foreach (string field in new[] { "clearCount", "bestWave", "bestScore", "constraints" })
            {
                PlayerPrefs.DeleteKey(Key(campaign, field));
            }
            PlayerPrefs.Save();
        }

        private static string Key(CampaignDefinition campaign, string field)
        {
            return $"run.{campaign.name}.{field}";
        }
    }
}
