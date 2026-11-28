using TowerDefense.Controls;
using TowerDefense.Game;
using TowerDefense.Grid;
using TowerDefense.Structures;
using TowerDefense.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 배치 모드의 조작 흐름. 평소에는 설치한 것을 클릭해 선택하고, 하단 막대나 숫자 키로 설치 모드에 들어간다.
    /// 설치 모드에서는 가리킨 칸에 미리보기를 보여주며, 설치하면 설치 모드가 풀린다 (연속 설치 키를 누르면 유지).
    /// 타워와 건물처럼 설치물이면 모두 같은 흐름을 쓴다.
    /// </summary>
    public class PlacementController : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private BuildGrid grid;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private BuildMenu menu;
        [SerializeField] private StructureBuilder builder;
        [SerializeField] private PlacementPreview preview;
        [SerializeField] private BuildBar bar;
        [SerializeField] private ResearchPanel researchPanel;
        [SerializeField] private ConstraintPanel constraintPanel;
        [SerializeField] private GameSpeed speed;

        /// <summary>설치하려고 고른 것. 설치 모드가 아니면 null.</summary>
        public StructureDefinition Placing { get; private set; }
        /// <summary>선택한 설치물. 없으면 null.</summary>
        public Structure Selected { get; private set; }

        /// <summary>설치 모드에 들어간다. 해금한 것만 고를 수 있다.</summary>
        public bool BeginPlacing(StructureDefinition definition)
        {
            if (modeController.CurrentMode != GameMode.Build || constraintPanel.IsOpen) return false;
            if (definition == null || definition.Prefab == null || !builder.IsUnlocked(definition)) return false;

            researchPanel.SetOpen(false);
            Selected = null;
            Placing = definition;
            return true;
        }

        /// <summary>설치 모드에 들어가거나, 같은 것을 다시 고르면 설치 모드를 끝낸다.</summary>
        public void TogglePlacing(StructureDefinition definition)
        {
            if (definition != null && definition == Placing) Cancel();
            else BeginPlacing(definition);
        }

        /// <summary>설치 모드와 선택을 모두 끝낸다.</summary>
        public void Cancel()
        {
            Placing = null;
            Selected = null;
        }

        /// <summary>선택한 설치물을 제거하고 환급받는다.</summary>
        public bool RemoveSelected()
        {
            if (Selected == null || !builder.TryRemove(Selected.Cell)) return false;

            Selected = null;
            return true;
        }

        private void OnEnable()
        {
            for (int i = 0; i < GameInput.SlotCount; i++)
            {
                input.Slot(i).performed += OnSlot;
            }
            modeController.ModeChanged += OnModeChanged;
        }

        private void OnDisable()
        {
            for (int i = 0; i < GameInput.SlotCount; i++)
            {
                input.Slot(i).performed -= OnSlot;
            }
            modeController.ModeChanged -= OnModeChanged;
            Cancel();
            if (preview != null) preview.Hide();
        }

        private void OnSlot(InputAction.CallbackContext context)
        {
            for (int i = 0; i < GameInput.SlotCount && i < menu.Entries.Count; i++)
            {
                if (input.Slot(i) == context.action) TogglePlacing(menu.Entries[i]);
            }
        }

        private void OnModeChanged(GameMode mode)
        {
            Cancel();
        }

        // 막대와 맵의 클릭을 한곳에서 처리해, 한 번의 클릭이 양쪽에 전달되지 않게 한다
        private void Update()
        {
            if (modeController.CurrentMode != GameMode.Build)
            {
                preview.Hide();
                return;
            }

            // 연구 화면을 열면 설치 모드와 선택을 끝낸다. 다른 선택 화면이 열린 동안에는 상태를 그대로 둔다
            if (researchPanel.IsOpen) Cancel();
            if (input.IsMenuOnly || researchPanel.IsOpen || constraintPanel.IsOpen)
            {
                preview.Hide();
                return;
            }

            if (input.Cancel.WasPressedThisFrame()) Cancel();

            Vector2 pointer = input.Point.ReadValue<Vector2>();
            bool pressed = input.Place.WasPressedThisFrame();

            BuildBarTarget target = bar.GetTarget(pointer, out int slot);
            if (target != BuildBarTarget.None)
            {
                if (pressed) ClickBar(target, slot);
                ShowSelected();
                return;
            }

            bool onGrid = TryGetPointedCell(pointer, out Vector2Int cell);
            if (Placing != null) UpdatePlacing(onGrid, cell, pressed);
            else UpdateSelecting(onGrid, cell, pressed);
        }

        private void ClickBar(BuildBarTarget target, int slot)
        {
            if (target == BuildBarTarget.Slot) TogglePlacing(menu.Entries[slot]);
            else if (target == BuildBarTarget.Remove) RemoveSelected();
            else if (target == BuildBarTarget.Speed) speed.Toggle();
        }

        private void UpdatePlacing(bool onGrid, Vector2Int cell, bool pressed)
        {
            if (!onGrid)
            {
                preview.Hide();
                return;
            }

            if (builder.TryGetStructure(cell, out Structure occupied))
            {
                preview.ShowRange(grid.CellToWorld(cell), occupied.Range);
                return;
            }

            bool buildable = builder.CanPlace(cell) && builder.CanAfford(Placing);
            preview.ShowGhost(Placing, grid.CellToWorld(cell), buildable, builder.GetRange(Placing));
            // 설치하지 못하면 설치 모드에 남는다
            if (!pressed || !builder.TryPlace(Placing, cell)) return;

            if (!input.KeepPlacing.IsPressed()) Placing = null;
        }

        private void UpdateSelecting(bool onGrid, Vector2Int cell, bool pressed)
        {
            Structure hovered = null;
            if (onGrid) builder.TryGetStructure(cell, out hovered);
            // 빈 곳을 클릭하면 선택이 풀린다
            if (pressed) Selected = hovered;

            if (Selected == null && hovered != null) preview.ShowRange(grid.CellToWorld(cell), hovered.Range);
            else ShowSelected();
        }

        private void ShowSelected()
        {
            if (Selected != null) preview.ShowRange(grid.CellToWorld(Selected.Cell), Selected.Range);
            else preview.Hide();
        }

        private bool TryGetPointedCell(Vector2 pointer, out Vector2Int cell)
        {
            cell = default;
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
