using TowerDefense.Combat;
using TowerDefense.Grid;
using TowerDefense.Towers;
using TowerDefense.Upgrades;
using UnityEngine;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 타워 설치·제거 규칙. 입력이나 화면 표시는 다루지 않는다. 설치 비용은 여기에 추가한다.
    /// </summary>
    public class TowerBuilder : MonoBehaviour
    {
        [SerializeField] private BuildGrid grid;
        [SerializeField] private Transform player;
        [SerializeField] private ResearchState research;

        /// <summary>지금 설치하면 적용될 공격 정보.</summary>
        public AttackSpec GetSpec(TowerDefinition definition)
        {
            return research.GetSpec(definition);
        }

        /// <summary>설치 가능한 칸인지 확인한다.</summary>
        public bool CanPlace(Vector2Int cell)
        {
            if (!grid.IsFree(cell)) return false;

            // 플레이어가 서 있는 칸에는 설치할 수 없다
            return player == null || grid.WorldToCell(player.position) != cell;
        }

        public bool TryPlace(TowerDefinition definition, Vector2Int cell)
        {
            if (definition == null || definition.Prefab == null || !CanPlace(cell)) return false;

            Tower tower = Instantiate(definition.Prefab, grid.CellToWorld(cell), Quaternion.identity, transform);
            tower.name = $"{definition.name} {cell}";
            tower.Initialize(definition, cell, research);
            grid.TryOccupy(cell, tower.gameObject);
            return true;
        }

        public bool TryRemove(Vector2Int cell)
        {
            if (!TryGetTower(cell, out Tower tower)) return false;

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
