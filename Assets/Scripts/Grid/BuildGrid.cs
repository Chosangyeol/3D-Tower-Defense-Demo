using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Grid
{
    /// <summary>
    /// XZ 평면의 건설 그리드. 이 오브젝트의 위치가 그리드 중심이다. (회전·스케일은 지원하지 않음)
    /// 칸은 비어 있거나, 건물이 점유했거나, 건설 금지(적 경로 등) 상태다.
    /// </summary>
    public class BuildGrid : MonoBehaviour
    {
        [SerializeField, Min(1)] private int width = 20;
        [SerializeField, Min(1)] private int height = 20;
        [SerializeField, Min(0.1f)] private float cellSize = 2f;

        private readonly Dictionary<Vector2Int, GameObject> occupants = new Dictionary<Vector2Int, GameObject>();

        private readonly HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;

        /// <summary>(0,0) 칸의 바깥쪽 모서리.</summary>
        public Vector3 Origin => transform.position - new Vector3(width * cellSize, 0f, height * cellSize) * 0.5f;

        public Vector2Int WorldToCell(Vector3 world)
        {
            Vector3 local = world - Origin;
            return new Vector2Int(Mathf.FloorToInt(local.x / cellSize), Mathf.FloorToInt(local.z / cellSize));
        }

        /// <summary>칸 중심의 월드 좌표.</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return Origin + new Vector3((cell.x + 0.5f) * cellSize, 0f, (cell.y + 0.5f) * cellSize);
        }

        public bool Contains(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
        }

        public bool IsFree(Vector2Int cell)
        {
            return Contains(cell) && !occupants.ContainsKey(cell) && !blocked.Contains(cell);
        }

        /// <summary>빈 칸을 건설 금지 칸으로 만든다.</summary>
        public bool Block(Vector2Int cell)
        {
            return IsFree(cell) && blocked.Add(cell);
        }

        public bool TryOccupy(Vector2Int cell, GameObject occupant)
        {
            if (occupant == null || !IsFree(cell)) return false;

            occupants.Add(cell, occupant);
            return true;
        }

        public bool TryGetOccupant(Vector2Int cell, out GameObject occupant)
        {
            return occupants.TryGetValue(cell, out occupant);
        }

        public bool Release(Vector2Int cell)
        {
            return occupants.Remove(cell);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
            Vector3 origin = Origin;
            for (int x = 0; x <= width; x++)
            {
                Vector3 start = origin + new Vector3(x * cellSize, 0f, 0f);
                Gizmos.DrawLine(start, start + new Vector3(0f, 0f, height * cellSize));
            }
            for (int z = 0; z <= height; z++)
            {
                Vector3 start = origin + new Vector3(0f, 0f, z * cellSize);
                Gizmos.DrawLine(start, start + new Vector3(width * cellSize, 0f, 0f));
            }
        }
    }
}
