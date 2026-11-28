using TowerDefense.Controls;
using TowerDefense.Economy;
using TowerDefense.Game;
using TowerDefense.Placement;
using TowerDefense.Structures;
using UnityEngine;

namespace TowerDefense.UI
{
    /// <summary>막대에서 포인터가 가리키는 곳.</summary>
    public enum BuildBarTarget
    {
        None,       // 막대 밖 (맵)
        Background, // 막대 위의 누를 것이 없는 곳
        Slot,       // 설치 목록의 한 칸
        Remove      // 선택한 설치물의 제거 버튼
    }

    /// <summary>
    /// 프로토타입용 임시 하단 설치 막대와 선택한 설치물의 정보. 화면만 그리며,
    /// 클릭은 PlacementController가 GetTarget으로 확인해 처리한다. 정식 UI가 생기면 교체한다.
    /// </summary>
    public class BuildBar : MonoBehaviour
    {
        private const float SlotWidth = 180f;
        private const float SlotHeight = 52f;
        private const float Gap = 6f;
        private const float Padding = 6f;
        // 화면 가장자리 스크롤 영역에 막대가 걸치지 않도록 띄운다
        private const float Margin = 16f;
        private const float InfoWidth = 440f;
        private const float InfoHeight = 104f;
        private const float HintHeight = 24f;

        [SerializeField] private GameModeController modeController;
        [SerializeField] private BuildMenu menu;
        [SerializeField] private StructureBuilder builder;
        [SerializeField] private PlacementController placement;
        [SerializeField] private ResearchPanel researchPanel;
        [SerializeField] private ConstraintPanel constraintPanel;

        private GUIStyle title;
        private GUIStyle small;
        private GUIStyle hint;

        public bool IsVisible =>
            modeController.CurrentMode == GameMode.Build && !researchPanel.IsOpen && !constraintPanel.IsOpen;

        // 칸이 많으면 화면 폭에 맞춰 좁힌다
        private float Width =>
            Mathf.Min(SlotWidth, (Screen.width - Margin * 2f - Padding * 2f) / Mathf.Max(menu.Entries.Count, 1) - Gap);

        private Rect BarRect
        {
            get
            {
                int count = menu.Entries.Count;
                float width = Width * count + Gap * Mathf.Max(count - 1, 0) + Padding * 2f;
                float height = SlotHeight + Padding * 2f;
                return new Rect((Screen.width - width) * 0.5f, Screen.height - height - Margin, width, height);
            }
        }

        private Rect InfoRect =>
            new Rect((Screen.width - InfoWidth) * 0.5f, BarRect.y - InfoHeight - Gap, InfoWidth, InfoHeight);

        /// <summary>설치 목록 칸의 영역. GUI 좌표(왼쪽 위 기준).</summary>
        public Rect GetSlotRect(int index)
        {
            Rect bar = BarRect;
            float width = Width;
            return new Rect(bar.x + Padding + (width + Gap) * index, bar.y + Padding, width, SlotHeight);
        }

        /// <summary>제거 버튼 영역. GUI 좌표.</summary>
        public Rect GetRemoveRect()
        {
            Rect info = InfoRect;
            return new Rect(info.x + 8f, info.yMax - 34f, info.width - 16f, 28f);
        }

        /// <summary>화면 좌표(왼쪽 아래 기준)가 가리키는 곳.</summary>
        /// <param name="slot">칸을 가리키면 그 번호, 아니면 -1</param>
        public BuildBarTarget GetTarget(Vector2 screenPoint, out int slot)
        {
            slot = -1;
            if (!IsVisible) return BuildBarTarget.None;

            var point = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            for (int i = 0; i < menu.Entries.Count; i++)
            {
                if (!GetSlotRect(i).Contains(point)) continue;

                slot = i;
                return BuildBarTarget.Slot;
            }
            if (BarRect.Contains(point)) return BuildBarTarget.Background;
            if (placement.Selected == null || !InfoRect.Contains(point)) return BuildBarTarget.None;

            return GetRemoveRect().Contains(point) ? BuildBarTarget.Remove : BuildBarTarget.Background;
        }

        private void OnGUI()
        {
            if (!IsVisible) return;

            EnsureStyles();
            GUI.Box(BarRect, GUIContent.none);
            for (int i = 0; i < menu.Entries.Count; i++)
            {
                DrawSlot(i, menu.Entries[i]);
            }

            if (placement.Selected != null)
            {
                DrawInfo(placement.Selected);
            }
            else if (placement.Placing != null)
            {
                var area = new Rect((Screen.width - InfoWidth) * 0.5f, BarRect.y - HintHeight - Gap, InfoWidth, HintHeight);
                GUI.Box(area, GUIContent.none);
                GUI.Label(area, $"설치 중: {placement.Placing.DisplayName} · Shift: 연속 설치 · 우클릭/ESC: 취소", hint);
            }
        }

        private void DrawSlot(int index, StructureDefinition definition)
        {
            if (definition == null) return;

            Rect slot = GetSlotRect(index);
            if (definition == placement.Placing) Fill(slot, new Color(0.35f, 0.6f, 1f, 0.55f));
            GUI.Box(slot, GUIContent.none);

            bool unlocked = builder.IsUnlocked(definition);
            Color previous = GUI.color;
            if (!unlocked) GUI.color = new Color(1f, 1f, 1f, 0.5f);

            string key = index < GameInput.SlotCount ? $"{index + 1}  " : "";
            GUI.Label(new Rect(slot.x + 6f, slot.y + 4f, slot.width - 12f, 22f), key + definition.DisplayName, title);

            string detail = unlocked ? ResourceAmount.Describe(builder.GetCost(definition)) : "잠김 · 연구에서 해금";
            // 자원이 모자라면 비용을 붉게 보여준다
            if (unlocked && !builder.CanAfford(definition)) GUI.color = new Color(1f, 0.45f, 0.4f);
            GUI.Label(new Rect(slot.x + 6f, slot.y + 28f, slot.width - 12f, 20f), detail, small);
            GUI.color = previous;
        }

        private void DrawInfo(Structure structure)
        {
            Rect info = InfoRect;
            GUI.Box(info, GUIContent.none);
            GUI.Box(info, GUIContent.none);

            GUI.Label(new Rect(info.x + 8f, info.y + 4f, info.width - 16f, 22f), structure.Definition.DisplayName, title);
            GUI.Label(new Rect(info.x + 8f, info.y + 28f, info.width - 16f, 40f), structure.Describe(), small);
            string refund = ResourceAmount.Describe(builder.GetRefund(structure));
            GUI.Box(GetRemoveRect(), $"제거 (환급: {refund})", GUI.skin.button);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void EnsureStyles()
        {
            if (title != null) return;

            title = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = false };
            small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, padding = new RectOffset(0, 0, 0, 0) };
            hint = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
        }
    }
}
