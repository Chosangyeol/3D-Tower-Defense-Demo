using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Upgrades
{
    /// <summary>
    /// 연구 하나. 완료하면 대상 타워 종류 전체에 효과가 적용된다. 연구 비용은 자원 시스템이 정해지면 추가한다.
    /// </summary>
    [GameDataCategory("업그레이드", "Upgrades", 60)]
    [CreateAssetMenu(menuName = "Tower Defense/Upgrade Definition", fileName = "Upgrade")]
    public class UpgradeDefinition : GameData
    {
        [SerializeField, TextArea] private string description;
        [SerializeField] private TowerDefinition target;
        [Tooltip("먼저 완료해야 하는 연구")]
        [SerializeField] private UpgradeDefinition[] prerequisites = new UpgradeDefinition[0];
        [SerializeField] private UpgradeEffect[] effects = new UpgradeEffect[0];

        public string Description => description;
        public TowerDefinition Target => target;
        public IReadOnlyList<UpgradeDefinition> Prerequisites => prerequisites;
        public IReadOnlyList<UpgradeEffect> Effects => effects;
    }
}
