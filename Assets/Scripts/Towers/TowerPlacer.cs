using TowerDefense.Core;
using TowerDefense.Grid;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 배치 모드의 타워 설치·제거. 빈 칸에는 고스트와 사거리를, 설치된 칸에는 그 타워의 사거리를 보여준다.
    /// </summary>
    public class TowerPlacer : MonoBehaviour
    {
        [SerializeField] private GameModeController modeController;
        [SerializeField] private BuildGrid grid;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform player;
        [SerializeField] private RangeIndicator rangeIndicator;
        [SerializeField] private TowerDefinition[] towers;

        [Header("고스트")]
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        [Header("입력")]
        [SerializeField] private InputAction pointAction = new InputAction("Point", InputActionType.Value, "<Mouse>/position");
        [SerializeField] private InputAction placeAction = new InputAction("Place", InputActionType.Button, "<Mouse>/leftButton");
        [SerializeField] private InputAction removeAction = new InputAction("Remove", InputActionType.Button, "<Mouse>/rightButton");

        private int selectedIndex;
        private Tower ghost;
        private Renderer[] ghostRenderers;

        private TowerDefinition Selected =>
            towers != null && selectedIndex < towers.Length ? towers[selectedIndex] : null;

        private void OnEnable()
        {
            pointAction.Enable();
            placeAction.Enable();
            removeAction.Enable();
        }

        private void OnDisable()
        {
            pointAction.Disable();
            placeAction.Disable();
            removeAction.Disable();
            HidePreview();
        }

        private void Update()
        {
            if (modeController.CurrentMode != GameMode.Build)
            {
                HidePreview();
                return;
            }

            ReadSelection();

            if (!TryGetPointedCell(out Vector2Int cell))
            {
                HidePreview();
                return;
            }

            if (grid.TryGetOccupant(cell, out GameObject occupant))
            {
                UpdateOccupiedCell(cell, occupant);
            }
            else
            {
                UpdateFreeCell(cell);
            }
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
            tower.Initialize(definition, cell);
            grid.TryOccupy(cell, tower.gameObject);
            return true;
        }

        public bool TryRemove(Vector2Int cell)
        {
            if (!grid.TryGetOccupant(cell, out GameObject occupant) || !occupant.TryGetComponent(out Tower _)) return false;

            grid.Release(cell);
            Destroy(occupant);
            return true;
        }

        private void UpdateFreeCell(Vector2Int cell)
        {
            TowerDefinition definition = Selected;
            if (definition == null || definition.Prefab == null)
            {
                HidePreview();
                return;
            }

            Vector3 position = grid.CellToWorld(cell);
            EnsureGhost(definition);
            ghost.transform.position = position;
            ghost.gameObject.SetActive(true);
            SetGhostMaterial(CanPlace(cell) ? validMaterial : invalidMaterial);
            rangeIndicator.Show(position, definition.Range);

            if (placeAction.WasPressedThisFrame()) TryPlace(definition, cell);
        }

        private void UpdateOccupiedCell(Vector2Int cell, GameObject occupant)
        {
            if (ghost != null) ghost.gameObject.SetActive(false);

            if (occupant.TryGetComponent(out Tower tower) && tower.Definition != null)
            {
                rangeIndicator.Show(grid.CellToWorld(cell), tower.Definition.Range);
                if (removeAction.WasPressedThisFrame()) TryRemove(cell);
            }
            else
            {
                rangeIndicator.Hide();
            }
        }

        private void ReadSelection()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || towers == null) return;

            for (int i = 0; i < towers.Length && i < 9; i++)
            {
                if (i == selectedIndex || !keyboard[Key.Digit1 + i].wasPressedThisFrame) continue;

                selectedIndex = i;
                DestroyGhost();
            }
        }

        private bool TryGetPointedCell(out Vector2Int cell)
        {
            cell = default;
            Vector2 pointer = pointAction.ReadValue<Vector2>();
            if (!viewCamera.pixelRect.Contains(pointer)) return false;

            // 콜라이더 대신 그리드 높이의 평면과 교차시켜 타워에 가려지지 않게 한다
            Ray ray = viewCamera.ScreenPointToRay(pointer);
            var plane = new Plane(Vector3.up, grid.transform.position);
            if (!plane.Raycast(ray, out float distance)) return false;

            cell = grid.WorldToCell(ray.GetPoint(distance));
            return grid.Contains(cell);
        }

        private void EnsureGhost(TowerDefinition definition)
        {
            if (ghost != null) return;

            ghost = Instantiate(definition.Prefab, transform);
            ghost.name = "Ghost";
            ghost.enabled = false;
            foreach (Collider ghostCollider in ghost.GetComponentsInChildren<Collider>())
            {
                ghostCollider.enabled = false;
            }
            ghostRenderers = ghost.GetComponentsInChildren<Renderer>();
            foreach (Renderer ghostRenderer in ghostRenderers)
            {
                ghostRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private void SetGhostMaterial(Material material)
        {
            if (material == null) return;

            foreach (Renderer ghostRenderer in ghostRenderers)
            {
                ghostRenderer.sharedMaterial = material;
            }
        }

        private void HidePreview()
        {
            if (ghost != null) ghost.gameObject.SetActive(false);
            if (rangeIndicator != null) rangeIndicator.Hide();
        }

        private void DestroyGhost()
        {
            if (ghost == null) return;

            Destroy(ghost.gameObject);
            ghost = null;
            ghostRenderers = null;
        }
    }
}
