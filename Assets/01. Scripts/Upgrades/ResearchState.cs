using System;
using System.Collections.Generic;
using TowerDefense.Combat;
using TowerDefense.Economy;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Upgrades
{
    /// <summary>
    /// 이번 판에서 완료한 연구. 타워 종류별 최종 공격 정보를 계산해서 설치된 타워와 새 타워가 함께 쓴다.
    /// </summary>
    public class ResearchState : MonoBehaviour
    {
        [SerializeField] private ResourceWallet wallet;

        private readonly List<UpgradeDefinition> completed = new List<UpgradeDefinition>();
        private readonly Dictionary<TowerDefinition, AttackSpec> specs = new Dictionary<TowerDefinition, AttackSpec>();

        /// <summary>연구를 완료할 때 호출된다.</summary>
        public event Action<UpgradeDefinition> Completed;

        public bool IsCompleted(UpgradeDefinition upgrade)
        {
            return completed.Contains(upgrade);
        }

        /// <summary>아직 완료하지 않았고 선행 연구를 모두 마쳤는지 확인한다.</summary>
        public bool CanComplete(UpgradeDefinition upgrade)
        {
            if (upgrade == null || IsCompleted(upgrade)) return false;

            foreach (UpgradeDefinition required in upgrade.Prerequisites)
            {
                if (required != null && !IsCompleted(required)) return false;
            }
            return true;
        }

        /// <summary>비용을 내고 연구한다.</summary>
        public bool TryResearch(UpgradeDefinition upgrade)
        {
            if (!CanComplete(upgrade) || !wallet.TrySpend(upgrade.Cost)) return false;

            return TryComplete(upgrade);
        }

        /// <summary>비용 없이 완료 처리한다. 보상으로 주는 연구 등에 사용한다.</summary>
        public bool TryComplete(UpgradeDefinition upgrade)
        {
            if (!CanComplete(upgrade)) return false;

            completed.Add(upgrade);
            specs.Clear();
            Completed?.Invoke(upgrade);
            return true;
        }

        public AttackSpec GetSpec(TowerDefinition tower)
        {
            if (specs.TryGetValue(tower, out AttackSpec spec)) return spec;

            spec = tower.CreateSpec();
            foreach (UpgradeDefinition upgrade in completed)
            {
                if (upgrade.Target != tower) continue;

                foreach (UpgradeEffect effect in upgrade.Effects)
                {
                    if (effect != null) effect.Apply(spec);
                }
            }
            specs.Add(tower, spec);
            return spec;
        }
    }
}
