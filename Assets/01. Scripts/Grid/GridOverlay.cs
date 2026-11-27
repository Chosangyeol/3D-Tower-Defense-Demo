using TowerDefense.Game;
using UnityEngine;
using UnityEngine.Rendering;

namespace TowerDefense.Grid
{
    /// <summary>
    /// 배치 모드에서만 그리드 선을 보여준다.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GridOverlay : MonoBehaviour
    {
        [SerializeField] private GameModeController modeController;
        [SerializeField] private BuildGrid grid;
        [SerializeField] private float heightOffset = 0.02f;

        private MeshRenderer meshRenderer;
        private Mesh mesh;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            mesh = BuildMesh();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private void OnEnable()
        {
            modeController.ModeChanged += Apply;
        }

        private void Start()
        {
            Apply(modeController.CurrentMode);
        }

        private void OnDisable()
        {
            modeController.ModeChanged -= Apply;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }

        private void Apply(GameMode mode)
        {
            meshRenderer.enabled = mode == GameMode.Build;
        }

        private Mesh BuildMesh()
        {
            int lineCount = grid.Width + grid.Height + 2;
            var vertices = new Vector3[lineCount * 2];
            var indices = new int[lineCount * 2];
            Vector3 origin = grid.Origin + Vector3.up * heightOffset;
            float totalWidth = grid.Width * grid.CellSize;
            float totalHeight = grid.Height * grid.CellSize;
            int v = 0;

            for (int x = 0; x <= grid.Width; x++)
            {
                Vector3 start = origin + new Vector3(x * grid.CellSize, 0f, 0f);
                vertices[v++] = transform.InverseTransformPoint(start);
                vertices[v++] = transform.InverseTransformPoint(start + new Vector3(0f, 0f, totalHeight));
            }
            for (int z = 0; z <= grid.Height; z++)
            {
                Vector3 start = origin + new Vector3(0f, 0f, z * grid.CellSize);
                vertices[v++] = transform.InverseTransformPoint(start);
                vertices[v++] = transform.InverseTransformPoint(start + new Vector3(totalWidth, 0f, 0f));
            }
            for (int i = 0; i < indices.Length; i++) indices[i] = i;

            var result = new Mesh { name = "GridLines" };
            result.vertices = vertices;
            result.SetIndices(indices, MeshTopology.Lines, 0);
            return result;
        }
    }
}
