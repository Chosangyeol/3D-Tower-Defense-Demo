using System.Collections.Generic;
using TowerDefense.Combat;
using TowerDefense.Economy;
using TowerDefense.Game;
using TowerDefense.Grid;
using TowerDefense.Towers;
using TowerDefense.Upgrades;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 타워 설치·제거 규칙. 입력이나 화면 표시는 다루지 않는다.
    /// 설치할 때 비용을 내고, 제거하면 준비 시간에는 전액, 웨이브 진행 중에는 일부를 돌려받는다.
    /// </summary>
    public class TowerBuilder : MonoBehaviour
    {
        [SerializeField] private BuildGrid grid;
        [SerializeField] private Transform player;
        [SerializeField] private ResearchState research;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private StageController stageController;

        /// <summary>지금 설치하면 적용될 공격 정보.</summary>
        public AttackSpec GetSpec(TowerDefinition definition)
        {
            return research.GetSpec(definition);
        }

        public bool CanAfford(TowerDefinition definition)
        {
            return definition != null && wallet.CanAfford(definition.Cost);
        }

        /// <summary>지금 제거하면 돌려받는 자원.</summary>
        public List<ResourceAmount> GetRefund(TowerDefinition definition)
        {
            float rate = waveSpawner.IsWaveRunning ? stageController.Stage.CombatRefundRate : 1f;
            var refund = new List<ResourceAmount>();
            foreach (ResourceAmount item in definition.Cost)
            {
                // 소수점은 버린다. 0.0001은 실수 계산 오차로 1이 깎이는 것을 막는다
                refund.Add(new ResourceAmount(item.resource, Mathf.FloorToInt(item.amount * rate + 0.0001f)));
            }
            return refund;
        }

        /// <summary>설치 가능한 칸인지 확인한다. 비용은 따로 확인한다.</summary>
        public bool CanPlace(Vector2Int cell)
        {
            if (!grid.IsFree(cell)) return false;

            // 플레이어가 서 있는 칸에는 설치할 수 없다
            return player == null || grid.WorldToCell(player.position) != cell;
        }

        public bool TryPlace(TowerDefinition definition, Vector2Int cell)
        {
            if (definition == null || definition.Prefab == null || !CanPlace(cell)) return false;
            if (!wallet.TrySpend(definition.Cost)) return false;

            Tower tower = Instantiate(definition.Prefab, grid.CellToWorld(cell), Quaternion.identity, transform);
            tower.name = $"{definition.name} {cell}";
            tower.Initialize(definition, cell, research);
            grid.TryOccupy(cell, tower.gameObject);
            return true;
        }

        public bool TryRemove(Vector2Int cell)
        {
            if (!TryGetTower(cell, out Tower tower)) return false;

            if (tower.Definition != null) wallet.Add(GetRefund(tower.Definition));
            grid.Release(cell);
            Destroy(tower.gameObject);
            return true;
        }

        /// <summary>칸에 설치된 타워를 찾는다.</summary>
        public bool TryGetTower(Vector2Int cell, out Tower tower)
        {
            tower = null;
            return grid.TryGetOccupant(cell, out GameObject occupant) && occupant.TryGetComponent(out tower);
        }
    }
}
