using System.Collections.Generic;
using TowerDefense.Controls;
using TowerDefense.Economy;
using TowerDefense.Game;
using TowerDefense.Upgrades;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.UI
{
    /// <summary>
    /// 프로토타입용 임시 연구 화면. 배치 모드에서 연구 키로 열고, 항목의 버튼을 눌러 연구한다.
    /// 클릭은 GameInput으로 처리한다. 정식 UI가 생기면 교체한다.
    /// </summary>
    public class ResearchPanel : MonoBehaviour
    {
        private const float Width = 420f;
        private const float Margin = 16f;
        private const float HeaderHeight = 28f;
        private const float EntryHeight = 96f;
        private const float ButtonWidth = 110f;

        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private GameSetup setup;
        [SerializeField] private ResearchState research;
        [SerializeField] private ResourceWallet wallet;

        public bool IsOpen { get; private set; }

        private IReadOnlyList<UpgradeDefinition> Upgrades => setup.Map.Upgrades;

        private Rect PanelRect =>
            new Rect(Screen.width - Width - Margin, Margin, Width, HeaderHeight + EntryHeight * Upgrades.Count + 8f);

        private void OnEnable()
        {
            input.Research.performed += OnToggle;
            input.Place.performed += OnClick;
            modeController.ModeChanged += OnModeChanged;
        }

        private void OnDisable()
        {
            input.Research.performed -= OnToggle;
            input.Place.performed -= OnClick;
            modeController.ModeChanged -= OnModeChanged;
            IsOpen = false;
        }

        public void SetOpen(bool open)
        {
            IsOpen = open && modeController.CurrentMode == GameMode.Build;
        }

        /// <summary>화면 좌표(왼쪽 아래 기준)가 열린 연구 화면 위에 있는지 확인한다.</summary>
        public bool IsPointerOver(Vector2 screenPoint)
        {
            return IsOpen && PanelRect.Contains(ToGui(screenPoint));
        }

        /// <summary>항목의 연구 버튼 영역. GUI 좌표(왼쪽 위 기준).</summary>
        public Rect GetButtonRect(int index)
        {
            Rect entry = GetEntryRect(index);
            return new Rect(entry.xMax - ButtonWidth - 8f, entry.yMax - 36f, ButtonWidth, 28f);
        }

        private Rect GetEntryRect(int index)
        {
            Rect panel = PanelRect;
            return new Rect(panel.x + 4f, panel.y + HeaderHeight + EntryHeight * index, panel.width - 8f, EntryHeight - 4f);
        }

        private static Vector2 ToGui(Vector2 screenPoint)
        {
            return new Vector2(screenPoint.x, Screen.height - screenPoint.y);
        }

        private void OnToggle(InputAction.CallbackContext context)
        {
            SetOpen(!IsOpen);
        }

        private void OnModeChanged(GameMode mode)
        {
            if (mode != GameMode.Build) IsOpen = false;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (!IsOpen) return;

            Vector2 pointer = ToGui(input.Point.ReadValue<Vector2>());
            for (int i = 0; i < Upgrades.Count; i++)
            {
                if (GetButtonRect(i).Contains(pointer)) research.TryResearch(Upgrades[i]);
            }
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.Box(PanelRect, "연구  (U: 닫기)");
            for (int i = 0; i < Upgrades.Count; i++)
            {
                UpgradeDefinition upgrade = Upgrades[i];
                if (upgrade == null) continue;

                Rect entry = GetEntryRect(i);
                GUI.Box(entry, GUIContent.none);
                var line = new Rect(entry.x + 8f, entry.y + 4f, entry.width - 16f, 20f);
                string target = upgrade.Target != null ? upgrade.Target.DisplayName : "?";
                GUI.Label(line, $"{upgrade.DisplayName}  [{target}]");
                line.y += 20f;
                GUI.Label(line, upgrade.Description);
                line.y += 20f;
                GUI.Label(line, "비용: " + ResourceAmount.Describe(upgrade.Cost));

                bool available = research.CanComplete(upgrade) && wallet.CanAfford(upgrade.Cost);
                bool previous = GUI.enabled;
                GUI.enabled = available;
                GUI.Box(GetButtonRect(i), DescribeState(upgrade), GUI.skin.button);
                GUI.enabled = previous;
            }
        }

        private string DescribeState(UpgradeDefinition upgrade)
        {
            if (research.IsCompleted(upgrade)) return "완료";

            foreach (UpgradeDefinition required in upgrade.Prerequisites)
            {
                if (required != null && !research.IsCompleted(required)) return $"{required.DisplayName} 필요";
            }
            return wallet.CanAfford(upgrade.Cost) ? "연구하기" : "자원 부족";
        }
    }
}
