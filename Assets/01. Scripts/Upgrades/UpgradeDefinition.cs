using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Upgrades
{
    /// <summary>
    /// 연구 트리의 항목 하나. 자원을 써서 완료하면 효과가 적용되고, 이 항목을 선행으로 둔 연구가 열린다.
    /// 갈래에 타워가 있으면 타워 효과는 그 타워에만, 없으면 모든 타워에 적용된다.
    /// 맵 데이터의 연구 목록에 넣어야 연구 화면에 나온다.
    /// </summary>
    [GameDataCategory("연구", "Upgrades", 60)]
    [CreateAssetMenu(menuName = "Tower Defense/Upgrade Definition", fileName = "Upgrade")]
    public class UpgradeDefinition : GameData
    {
        [Tooltip("비우면 효과와 해금 내용으로 자동으로 만든다")]
        [SerializeField, TextArea] private string description;
        [Tooltip("연구 화면에서 놓일 갈래")]
        [SerializeField] private ResearchBranchDefinition branch;
        [SerializeField] private ResourceAmount[] cost = new ResourceAmount[0];
        [Tooltip("먼저 완료해야 하는 연구 (모두 필요)")]
        [SerializeField] private UpgradeDefinition[] prerequisites = new UpgradeDefinition[0];
        [Tooltip("같은 갈래에서 이 이름이 같은 연구는 하나만 완료할 수 있다 (택 1). 비우면 일반 연구")]
        [SerializeField] private string choiceGroup;
        [SerializeField] private GameEffect[] effects = new GameEffect[0];
        [Tooltip("완료하면 설치와 전용 연구가 열리는 타워. 어느 연구든 여기에 적힌 타워는 처음에 잠겨 있다")]
        [SerializeField] private TowerDefinition[] unlockTowers = new TowerDefinition[0];

        public string Description => string.IsNullOrWhiteSpace(description) ? DescribeContents() : description;
        public ResearchBranchDefinition Branch => branch;
        /// <summary>효과가 적용될 타워. 모든 타워이거나 타워 연구가 아니면 null.</summary>
        public TowerDefinition Target => branch != null ? branch.Tower : null;
        public IReadOnlyList<ResourceAmount> Cost => cost;
        public IReadOnlyList<UpgradeDefinition> Prerequisites => prerequisites;
        public string ChoiceGroup => choiceGroup;
        public IReadOnlyList<GameEffect> Effects => effects;
        public IReadOnlyList<TowerDefinition> UnlockTowers => unlockTowers;

        public bool Unlocks(TowerDefinition tower)
        {
            foreach (TowerDefinition unlocked in unlockTowers)
            {
                if (unlocked != null && unlocked == tower) return true;
            }
            return false;
        }

        /// <summary>둘 중 하나만 완료할 수 있는 사이인지 확인한다.</summary>
        public bool ConflictsWith(UpgradeDefinition other)
        {
            return other != null && other != this && !string.IsNullOrWhiteSpace(choiceGroup)
                   && other.branch == branch && other.choiceGroup == choiceGroup;
        }

        // 수치를 고치면 설명도 따라 바뀌도록 효과에서 만든다
        private string DescribeContents()
        {
            var parts = new List<string>();
            foreach (TowerDefinition tower in unlockTowers)
            {
                if (tower != null) parts.Add($"{tower.DisplayName} 설치·연구 해금");
            }
            foreach (GameEffect effect in effects)
            {
                if (effect == null) continue;

                string text = effect.Describe();
                if (!string.IsNullOrEmpty(text)) parts.Add(text);
            }
            return string.Join(", ", parts);
        }
    }
}
