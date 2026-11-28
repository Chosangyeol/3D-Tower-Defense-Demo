using System;
using System.Collections.Generic;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Game;
using TowerDefense.Structures;
using UnityEngine;

namespace TowerDefense.Upgrades
{
    /// <summary>연구 하나를 지금 진행할 수 있는지, 없다면 이유. 비용은 따로 확인한다.</summary>
    public enum ResearchStatus
    {
        Available,
        Completed,
        TowerLocked,        // 갈래의 타워가 아직 해금되지 않음
        OtherChosen,        // 택 1에서 다른 항목을 골랐음
        MissingPrerequisite
    }

    /// <summary>
    /// 이번 판에서 완료한 연구와 그에 따라 해금된 설치물. 완료하면 효과를 RunModifiers에 더해
    /// 설치된 타워와 새 타워에 함께 적용한다.
    /// </summary>
    public class ResearchState : MonoBehaviour
    {
        [SerializeField] private GameSetup setup;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private RunModifiers modifiers;

        private readonly List<UpgradeDefinition> completed = new List<UpgradeDefinition>();

        /// <summary>연구를 완료할 때 호출된다.</summary>
        public event Action<UpgradeDefinition> Completed;

        /// <summary>이번 맵의 연구 목록.</summary>
        public IReadOnlyList<UpgradeDefinition> All => setup.Map.Upgrades;

        public bool IsCompleted(UpgradeDefinition upgrade)
        {
            return completed.Contains(upgrade);
        }

        /// <summary>설치할 수 있는지 여부. 해금 연구가 없는 설치물은 처음부터 열려 있다.</summary>
        public bool IsUnlocked(StructureDefinition structure)
        {
            if (structure == null) return false;

            foreach (UpgradeDefinition upgrade in completed)
            {
                if (upgrade.Unlocks(structure)) return true;
            }
            foreach (UpgradeDefinition upgrade in All)
            {
                if (upgrade != null && upgrade.Unlocks(structure)) return false;
            }
            return true;
        }

        public ResearchStatus GetStatus(UpgradeDefinition upgrade)
        {
            if (IsCompleted(upgrade)) return ResearchStatus.Completed;
            if (upgrade.Target != null && !IsUnlocked(upgrade.Target)) return ResearchStatus.TowerLocked;

            foreach (UpgradeDefinition done in completed)
            {
                if (upgrade.ConflictsWith(done)) return ResearchStatus.OtherChosen;
            }
            foreach (UpgradeDefinition required in upgrade.Prerequisites)
            {
                if (required != null && !IsCompleted(required)) return ResearchStatus.MissingPrerequisite;
            }
            return ResearchStatus.Available;
        }

        public bool CanComplete(UpgradeDefinition upgrade)
        {
            return upgrade != null && GetStatus(upgrade) == ResearchStatus.Available;
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
            modifiers.Apply(upgrade.Effects, upgrade.Target);
            Completed?.Invoke(upgrade);
            return true;
        }

        // 데이터를 고치다 생긴 문제는 판을 시작할 때 알려준다
        private void Awake()
        {
            foreach (string problem in ResearchProblems.Find(All))
            {
                Debug.LogWarning($"[연구 데이터] {problem}", setup.Map);
            }
        }
    }
}
