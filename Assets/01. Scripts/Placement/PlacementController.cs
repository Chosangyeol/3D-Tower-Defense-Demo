using TowerDefense.Controls;
using TowerDefense.Game;
using TowerDefense.Grid;
using TowerDefense.Towers;
using TowerDefense.UI;
using UnityEngine;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 배치 모드의 조작 흐름: 가리킨 칸을 찾아 미리보기를 보여주고, 클릭하면 설치·제거를 요청한다.
    /// </summary>
    public class PlacementController : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private BuildGrid grid;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private TowerSelection selection;
        [SerializeField] private TowerBuilder builder;
        [SerializeField] private PlacementPreview preview;
        [SerializeField] private ResearchPanel researchPanel;
        [SerializeField] private ConstraintPanel constraintPanel;

        private void OnDisable()
        {
            if (preview != null) preview.Hide();
        }

        private void Update()
        {
            // 연구 화면 위의 클릭, 제약 화면과 선택 화면이 열린 동안의 클릭은 뒤쪽 칸에 전달하지 않는다
            bool overPanel = input.IsMenuOnly || constraintPanel.IsOpen
                             || researchPanel.IsPointerOver(input.Point.ReadValue<Vector2>());
            if (modeController.CurrentMode != GameMode.Build || overPanel || !TryGetPointedCell(out Vector2Int cell))
            {
                preview.Hide();
                return;
            }

            if (builder.TryGetTower(cell, out Tower tower) && tower.Definition != null)
            {
                preview.ShowRange(grid.CellToWorld(cell), tower.Stats.Range);
                if (input.Remove.WasPressedThisFrame()) builder.TryRemove(cell);
                return;
            }

            TowerDefinition definition = selection.Selected;
            if (definition == null || definition.Prefab == null)
            {
                preview.Hide();
                return;
            }

            bool buildable = builder.CanPlace(cell) && builder.CanAfford(definition);
            preview.ShowGhost(definition, grid.CellToWorld(cell), buildable, builder.GetStats(definition).Range);
            if (input.Place.WasPressedThisFrame()) builder.TryPlace(definition, cell);
        }

        private bool TryGetPointedCell(out Vector2Int cell)
        {
            cell = default;
            Vector2 pointer = input.Point.ReadValue<Vector2>();
            if (!viewCamera.pixelRect.Contains(pointer)) return false;

            // 콜라이더 대신 그리드 높이의 평면과 교차시켜 타워에 가려지지 않게 한다
            Ray ray = viewCamera.ScreenPointToRay(pointer);
            var plane = new Plane(Vector3.up, grid.transform.position);
            if (!plane.Raycast(ray, out float distance)) return false;

            cell = grid.WorldToCell(ray.GetPoint(distance));
            return grid.Contains(cell);
        }
    }
}
