using TowerDefense.Data;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Upgrades
{
    /// <summary>연구 화면의 탭. 번호는 데이터에 저장되므로 바꾸지 않고 새 번호로 추가한다.</summary>
    public enum ResearchTab
    {
        [InspectorName("타워")] Tower = 0,
        [InspectorName("건물")] Building = 1,
        [InspectorName("플레이어")] Player = 2
    }

    public static class ResearchTabExtensions
    {
        public static string ToLabel(this ResearchTab tab)
        {
            switch (tab)
            {
                case ResearchTab.Tower: return "타워";
                case ResearchTab.Building: return "건물";
                case ResearchTab.Player: return "플레이어";
                default: return tab.ToString();
            }
        }
    }

    /// <summary>
    /// 연구 트리의 갈래 하나. 연구 화면에서 세로 한 줄이 되며, 어느 탭에 놓일지와 적용할 타워를 정한다.
    /// </summary>
    [GameDataCategory("연구 갈래", "ResearchBranches", 61)]
    [CreateAssetMenu(menuName = "Tower Defense/Research Branch", fileName = "ResearchBranch")]
    public class ResearchBranchDefinition : GameData
    {
        [SerializeField] private ResearchTab tab;
        [Tooltip("이 갈래의 연구가 적용될 타워. 타워가 해금되기 전에는 갈래 전체가 잠긴다. 비우면 모든 타워")]
        [SerializeField] private TowerDefinition tower;

        public ResearchTab Tab => tab;
        public TowerDefinition Tower => tower;
    }
}
