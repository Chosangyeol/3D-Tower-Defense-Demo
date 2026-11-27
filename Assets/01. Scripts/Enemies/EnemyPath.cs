using System.Collections.Generic;
using TowerDefense.Game;
using TowerDefense.Grid;
using UnityEngine;
using UnityEngine.Rendering;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// 적이 따라가는 고정 경로. 꺾이는 지점은 맵 데이터의 그리드 칸 좌표를 쓴다. (가로·세로 직선만)
    /// 경로가 지나는 칸은 건설 금지로 만들고, 바닥에 길을 표시한다.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class EnemyPath : MonoBehaviour
    {
        [SerializeField] private BuildGrid grid;
        [SerializeField] private GameSetup setup;
        [SerializeField] private float heightOffset = 0.01f;
        [SerializeField, Range(0.1f, 1f)] private float tileFill = 0.9f;
        [Tooltip("경로 끝에 놓인 기지의 반지름. 적은 이 앞에서 멈춘다")]
        [SerializeField, Min(0f)] private float goalRadius = 0.9f;

        private Mesh mesh;

        public int PointCount => Waypoints.Count;
        public float GoalRadius => goalRadius;

        /// <summary>시작점부터 끝점까지의 길이.</summary>
        public float Length
        {
            get
            {
                float length = 0f;
                for (int i = 1; i < PointCount; i++) length += Vector3.Distance(GetPoint(i - 1), GetPoint(i));
                return length;
            }
        }

        private IReadOnlyList<Vector2Int> Waypoints =>
            setup != null && setup.Map != null
                ? setup.Map.PathWaypoints
                : System.Array.Empty<Vector2Int>();

        public Vector3 GetPoint(int index)
        {
            return grid.CellToWorld(Waypoints[index]);
        }

        /// <summary>경로가 지나는 모든 칸. 시작점부터 순서대로이며 중복은 없다.</summary>
        public List<Vector2Int> GetCells()
        {
            var cells = new List<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            IReadOnlyList<Vector2Int> waypoints = Waypoints;

            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector2Int cell = waypoints[i];
                if (visited.Add(cell)) cells.Add(cell);
                if (i == waypoints.Count - 1) break;

                Vector2Int next = waypoints[i + 1];
                if (cell.x != next.x && cell.y != next.y)
                {
                    Debug.LogError($"경로의 {i}번과 {i + 1}번 지점이 직선이 아닙니다: {cell} → {next}", this);
                    continue;
                }

                var step = new Vector2Int(System.Math.Sign(next.x - cell.x), System.Math.Sign(next.y - cell.y));
                while (cell != next)
                {
                    cell += step;
                    if (visited.Add(cell)) cells.Add(cell);
                }
            }
            return cells;
        }

        private void Awake()
        {
            if (PointCount < 2) Debug.LogError("경로 지점이 2개 이상 필요합니다.", this);

            List<Vector2Int> cells = GetCells();
            foreach (Vector2Int cell in cells)
            {
                if (!grid.Block(cell)) Debug.LogError($"경로 칸을 막을 수 없습니다: {cell}", this);
            }

            GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            mesh = BuildMesh(cells);
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }

        private Mesh BuildMesh(List<Vector2Int> cells)
        {
            var vertices = new Vector3[cells.Count * 4];
            var normals = new Vector3[cells.Count * 4];
            var triangles = new int[cells.Count * 6];
            float half = grid.CellSize * tileFill * 0.5f;

            for (int i = 0; i < cells.Count; i++)
            {
                Vector3 center = grid.CellToWorld(cells[i]) + Vector3.up * heightOffset;
                int v = i * 4;
                vertices[v] = transform.InverseTransformPoint(center + new Vector3(-half, 0f, -half));
                vertices[v + 1] = transform.InverseTransformPoint(center + new Vector3(-half, 0f, half));
                vertices[v + 2] = transform.InverseTransformPoint(center + new Vector3(half, 0f, half));
                vertices[v + 3] = transform.InverseTransformPoint(center + new Vector3(half, 0f, -half));
                for (int n = 0; n < 4; n++) normals[v + n] = Vector3.up;

                int t = i * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;
            }

            var result = new Mesh { name = "PathTiles" };
            result.vertices = vertices;
            result.normals = normals;
            result.triangles = triangles;
            return result;
        }

        private void OnDrawGizmos()
        {
            if (grid == null) return;

            Gizmos.color = new Color(1f, 0.5f, 0.1f);
            for (int i = 0; i + 1 < PointCount; i++)
            {
                Gizmos.DrawLine(GetPoint(i) + Vector3.up * 0.2f, GetPoint(i + 1) + Vector3.up * 0.2f);
            }
        }
    }
}
