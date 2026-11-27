using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Upgrades
{
    /// <summary>
    /// 연구 하나. 자원을 써서 완료하면 대상 타워 종류 전체에 효과가 적용된다.
    /// </summary>
    [GameDataCategory("업그레이드", "Upgrades", 60)]
    [CreateAssetMenu(menuName = "Tower Defense/Upgrade Definition", fileName = "Upgrade")]
    public class UpgradeDefinition : GameData
    {
        [SerializeField, TextArea] private string description;
        [SerializeField] private TowerDefinition target;
        [SerializeField] private ResourceAmount[] cost = new ResourceAmount[0];
        [Tooltip("먼저 완료해야 하는 연구")]
        [SerializeField] private UpgradeDefinition[] prerequisites = new UpgradeDefinition[0];
        [SerializeField] private GameEffect[] effects = new GameEffect[0];

        public string Description => description;
        public TowerDefinition Target => target;
        public IReadOnlyList<ResourceAmount> Cost => cost;
        public IReadOnlyList<UpgradeDefinition> Prerequisites => prerequisites;
        public IReadOnlyList<GameEffect> Effects => effects;
    }
}
