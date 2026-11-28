using System.Collections.Generic;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Game;
using TowerDefense.Grid;
using TowerDefense.Structures;
using TowerDefense.Upgrades;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 설치물(타워, 건물)의 설치·제거 규칙. 입력이나 화면 표시는 다루지 않는다.
    /// 설치할 때 비용을 내고, 제거하면 준비 시간에는 전액, 웨이브 진행 중에는 일부를 돌려받는다.
    /// </summary>
    public class StructureBuilder : MonoBehaviour
    {
        [SerializeField] private BuildGrid grid;
        [SerializeField] private Transform player;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunModifiers modifiers;
        [SerializeField] private ResearchState research;

        /// <summary>지금 설치하면 적용될 범위 반경. 범위가 없으면 0.</summary>
        public float GetRange(StructureDefinition definition)
        {
            return definition.GetRange(modifiers);
        }

        /// <summary>지금 설치할 때 내야 하는 비용.</summary>
        public List<ResourceAmount> GetCost(StructureDefinition definition)
        {
            return definition.GetCost(modifiers);
        }

        /// <summary>연구로 해금해야 하는데 아직 하지 않았으면 false.</summary>
        public bool IsUnlocked(StructureDefinition definition)
        {
            return research.IsUnlocked(definition);
        }

        public bool CanAfford(StructureDefinition definition)
        {
            return definition != null && wallet.CanAfford(GetCost(definition));
        }

        /// <summary>지금 제거하면 돌려받는 자원.</summary>
        public List<ResourceAmount> GetRefund(Structure structure)
        {
            float rate = waveSpawner.IsWaveRunning ? setup.Map.CombatRefundRate : 1f;
            var refund = new List<ResourceAmount>();
            foreach (ResourceAmount item in structure.PaidCost)
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

        public bool TryPlace(StructureDefinition definition, Vector2Int cell)
        {
            if (definition == null || definition.Prefab == null || !IsUnlocked(definition) || !CanPlace(cell)) return false;
            List<ResourceAmount> cost = GetCost(definition);
            if (!wallet.TrySpend(cost)) return false;

            Structure structure = Instantiate(definition.Prefab, grid.CellToWorld(cell), Quaternion.identity, transform);
            structure.name = $"{definition.name} {cell}";
            structure.Initialize(definition, cell, modifiers, cost);
            grid.TryOccupy(cell, structure.gameObject);
            return true;
        }

        public bool TryRemove(Vector2Int cell)
        {
            if (!TryGetStructure(cell, out Structure structure)) return false;

            if (structure.Definition != null) wallet.Add(GetRefund(structure));
            grid.Release(cell);
            Destroy(structure.gameObject);
            return true;
        }

        /// <summary>칸에 설치된 것을 찾는다.</summary>
        public bool TryGetStructure(Vector2Int cell, out Structure structure)
        {
            structure = null;
            return grid.TryGetOccupant(cell, out GameObject occupant) && occupant.TryGetComponent(out structure);
        }
    }
}
