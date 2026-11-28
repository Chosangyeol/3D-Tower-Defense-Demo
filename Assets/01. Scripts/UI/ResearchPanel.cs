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
    /// 프로토타입용 임시 연구 화면. 배치 모드에서 연구 키로 열고, 탭을 골라 항목의 버튼을 눌러 연구한다.
    /// 갈래는 세로 한 줄이며 선행 연구가 깊을수록 아래에 놓인다. 배치는 연구 데이터에서 만든다.
    /// 화면보다 크면 줄여서 보여준다. 클릭은 GameInput으로 처리한다. 정식 UI가 생기면 교체한다.
    /// </summary>
    public class ResearchPanel : MonoBehaviour
    {
        private const float Margin = 16f;
        private const float Padding = 10f;
        private const float TitleHeight = 28f;
        private const float TabHeight = 28f;
        private const float TabWidth = 130f;
        private const float BranchHeight = 28f;
        private const float ColumnWidth = 250f;
        private const float NodeHeight = 84f;
        private const float NodeGap = 6f;
        private const float StepGap = 22f;
        private const float ButtonWidth = 90f;
        // 연구가 적어도 제목과 탭이 잘리지 않는 폭
        private const int MinColumns = 3;

        private static readonly ResearchTab[] Tabs = { ResearchTab.Tower, ResearchTab.Building, ResearchTab.Player };

        /// <summary>세로 한 줄. Steps는 위에서부터의 단계이고, 한 단계에 여러 연구가 있을 수 있다.</summary>
        private class Column
        {
            public ResearchBranchDefinition Branch;
            public readonly SortedDictionary<int, List<UpgradeDefinition>> Steps =
                new SortedDictionary<int, List<UpgradeDefinition>>();
        }

        private struct Node
        {
            public UpgradeDefinition Upgrade;
            public Rect Rect;
        }

        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private ResearchState research;
        [SerializeField] private ResourceWallet wallet;

        private GUIStyle heading;
        private GUIStyle small;
        private GUIStyle center;

        public bool IsOpen { get; private set; }
        public ResearchTab Tab { get; private set; }

        public void SetOpen(bool open)
        {
            IsOpen = open && modeController.CurrentMode == GameMode.Build;
        }

        public void SelectTab(ResearchTab tab)
        {
            Tab = tab;
        }

        /// <summary>화면 좌표(왼쪽 아래 기준)가 열린 연구 화면 위에 있는지 확인한다.</summary>
        public bool IsPointerOver(Vector2 screenPoint)
        {
            if (!IsOpen) return false;

            Vector2 size = GetPanelSize();
            return new Rect(Vector2.zero, size).Contains(ToPanel(screenPoint, size));
        }

        /// <summary>탭 버튼의 화면 영역. GUI 좌표(왼쪽 위 기준).</summary>
        public Rect GetTabRect(ResearchTab tab)
        {
            return ToScreen(GetLocalTabRect(System.Array.IndexOf(Tabs, tab)), GetPanelSize());
        }

        /// <summary>지금 탭에 있는 연구의 버튼 화면 영역. GUI 좌표. 지금 탭에 없으면 빈 영역.</summary>
        public Rect GetButtonRect(UpgradeDefinition upgrade)
        {
            Vector2 size = GetPanelSize();
            foreach (Node node in GetNodes(BuildColumns(Tab), size.x))
            {
                if (node.Upgrade == upgrade) return ToScreen(GetLocalButtonRect(node.Rect), size);
            }
            return default;
        }

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

            Vector2 size = GetPanelSize();
            Vector2 pointer = ToPanel(input.Point.ReadValue<Vector2>(), size);
            for (int i = 0; i < Tabs.Length; i++)
            {
                if (!GetLocalTabRect(i).Contains(pointer)) continue;

                SelectTab(Tabs[i]);
                return;
            }
            foreach (Node node in GetNodes(BuildColumns(Tab), size.x))
            {
                if (!GetLocalButtonRect(node.Rect).Contains(pointer)) continue;

                research.TryResearch(node.Upgrade);
                return;
            }
        }

        // 이 탭의 갈래를 연구 목록에 처음 나오는 순서로 모은다
        private List<Column> BuildColumns(ResearchTab tab)
        {
            var columns = new List<Column>();
            foreach (UpgradeDefinition upgrade in research.All)
            {
                if (upgrade == null || upgrade.Branch == null || upgrade.Branch.Tab != tab) continue;

                Column column = columns.Find(c => c.Branch == upgrade.Branch);
                if (column == null)
                {
                    column = new Column { Branch = upgrade.Branch };
                    columns.Add(column);
                }

                int depth = GetDepth(upgrade, new HashSet<UpgradeDefinition>());
                if (!column.Steps.TryGetValue(depth, out List<UpgradeDefinition> step))
                {
                    step = new List<UpgradeDefinition>();
                    column.Steps.Add(depth, step);
                }
                if (!step.Contains(upgrade)) step.Add(upgrade);
            }
            return columns;
        }

        // 같은 갈래의 선행 연구를 가장 길게 따라간 횟수. 선행이 서로를 가리켜도 멈춘다
        private static int GetDepth(UpgradeDefinition upgrade, HashSet<UpgradeDefinition> visiting)
        {
            if (!visiting.Add(upgrade)) return 0;

            int depth = 0;
            foreach (UpgradeDefinition required in upgrade.Prerequisites)
            {
                if (required == null || required.Branch != upgrade.Branch) continue;

                depth = Mathf.Max(depth, GetDepth(required, visiting) + 1);
            }
            visiting.Remove(upgrade);
            return depth;
        }

        private static float GetColumnHeight(Column column)
        {
            float height = StepGap * (column.Steps.Count - 1);
            foreach (List<UpgradeDefinition> step in column.Steps.Values)
            {
                height += NodeHeight * step.Count + NodeGap * (step.Count - 1);
            }
            return height;
        }

        // 탭을 바꿔도 크기가 그대로이도록 모든 탭 중 가장 큰 크기를 쓴다.
        // 크기가 줄면 탭을 누른 클릭이 뒤쪽 칸에 전달될 수 있다
        private Vector2 GetPanelSize()
        {
            int columnCount = MinColumns;
            float height = 0f;
            foreach (ResearchTab tab in Tabs)
            {
                List<Column> columns = BuildColumns(tab);
                columnCount = Mathf.Max(columnCount, columns.Count);
                foreach (Column column in columns) height = Mathf.Max(height, GetColumnHeight(column));
            }
            return new Vector2(ColumnWidth * columnCount + Padding * 2f,
                TitleHeight + TabHeight + BranchHeight + height + Padding * 2f);
        }

        private static float GetColumnX(int index, int count, float panelWidth)
        {
            // 갈래가 적은 탭은 가운데에 모은다
            return (panelWidth - ColumnWidth * count) * 0.5f + ColumnWidth * index;
        }

        private static List<Node> GetNodes(List<Column> columns, float panelWidth)
        {
            var nodes = new List<Node>();
            for (int i = 0; i < columns.Count; i++)
            {
                float x = GetColumnX(i, columns.Count, panelWidth) + 4f;
                float y = Padding + TitleHeight + TabHeight + BranchHeight;
                foreach (List<UpgradeDefinition> step in columns[i].Steps.Values)
                {
                    foreach (UpgradeDefinition upgrade in step)
                    {
                        nodes.Add(new Node { Upgrade = upgrade, Rect = new Rect(x, y, ColumnWidth - 8f, NodeHeight) });
                        y += NodeHeight + NodeGap;
                    }
                    y += StepGap - NodeGap;
                }
            }
            return nodes;
        }

        private static Rect GetLocalTabRect(int index)
        {
            return new Rect(Padding + (TabWidth + 6f) * index, Padding + TitleHeight, TabWidth, TabHeight - 4f);
        }

        private static Rect GetLocalButtonRect(Rect node)
        {
            return new Rect(node.xMax - ButtonWidth - 4f, node.y + 4f, ButtonWidth, 22f);
        }

        // 화면에 다 들어오지 않으면 줄인다
        private static float GetScale(Vector2 size)
        {
            return Mathf.Min(1f, (Screen.width - Margin * 2f) / size.x, (Screen.height - Margin * 2f) / size.y);
        }

        private static Vector2 GetOrigin(Vector2 size)
        {
            return (new Vector2(Screen.width, Screen.height) - size * GetScale(size)) * 0.5f;
        }

        private static Rect ToScreen(Rect local, Vector2 size)
        {
            float scale = GetScale(size);
            return new Rect(GetOrigin(size) + local.position * scale, local.size * scale);
        }

        // 화면 좌표(왼쪽 아래 기준)를 연구 화면 안의 좌표로 바꾼다
        private static Vector2 ToPanel(Vector2 screenPoint, Vector2 size)
        {
            var gui = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            return (gui - GetOrigin(size)) / GetScale(size);
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            EnsureStyles();
            Vector2 size = GetPanelSize();
            Vector2 origin = GetOrigin(size);
            float scale = GetScale(size);

            // 안내 화면보다 위, 선택 화면보다 아래에 그린다
            GUI.depth = -50;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(origin, Quaternion.identity, new Vector3(scale, scale, 1f));

            var panel = new Rect(Vector2.zero, size);
            Fill(panel, new Color(0.08f, 0.09f, 0.11f, 0.94f));
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(Padding, Padding, size.x - Padding * 2f, TitleHeight), DescribeTitle(), heading);

            for (int i = 0; i < Tabs.Length; i++)
            {
                Rect tab = GetLocalTabRect(i);
                if (Tabs[i] == Tab) Fill(tab, new Color(0.35f, 0.6f, 1f, 0.55f));
                GUI.Box(tab, Tabs[i].ToLabel(), GUI.skin.button);
            }

            DrawColumns(BuildColumns(Tab), size.x);
            GUI.matrix = previousMatrix;
        }

        private void DrawColumns(List<Column> columns, float panelWidth)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                Column column = columns[i];
                float x = GetColumnX(i, columns.Count, panelWidth);
                float y = Padding + TitleHeight + TabHeight;

                string title = column.Branch.DisplayName;
                if (column.Branch.Tower != null && !research.IsUnlocked(column.Branch.Tower)) title += " · 건물에서 해금";
                GUI.Label(new Rect(x, y, ColumnWidth, BranchHeight), title, center);

                // 단계 사이에 아래로 이어지는 표시
                y += BranchHeight;
                bool first = true;
                foreach (List<UpgradeDefinition> step in column.Steps.Values)
                {
                    if (!first) GUI.Label(new Rect(x, y - StepGap, ColumnWidth, StepGap), "▼", center);
                    first = false;
                    y += NodeHeight * step.Count + NodeGap * (step.Count - 1) + StepGap;
                }
            }
            foreach (Node node in GetNodes(columns, panelWidth)) DrawNode(node);
        }

        private void DrawNode(Node node)
        {
            UpgradeDefinition upgrade = node.Upgrade;
            ResearchStatus status = research.GetStatus(upgrade);
            bool affordable = wallet.CanAfford(upgrade.Cost);
            Rect rect = node.Rect;

            if (status == ResearchStatus.Completed) Fill(rect, new Color(0.2f, 0.65f, 0.3f, 0.45f));
            GUI.Box(rect, GUIContent.none);

            Color previous = GUI.color;
            bool locked = status != ResearchStatus.Available && status != ResearchStatus.Completed;
            if (locked) GUI.color = new Color(1f, 1f, 1f, 0.5f);

            string name = upgrade.DisplayName;
            if (!string.IsNullOrWhiteSpace(upgrade.ChoiceGroup)) name += " · 택 1";
            GUI.Label(new Rect(rect.x + 6f, rect.y + 4f, rect.width - ButtonWidth - 14f, 22f), name, heading);
            GUI.Label(new Rect(rect.x + 6f, rect.y + 28f, rect.width - 12f, 34f), upgrade.Description, small);
            GUI.Label(new Rect(rect.x + 6f, rect.y + 63f, rect.width - 12f, 20f), DescribeCost(upgrade), small);
            GUI.color = previous;

            bool enabled = GUI.enabled;
            GUI.enabled = status == ResearchStatus.Available && affordable;
            GUI.Box(GetLocalButtonRect(rect), DescribeState(status, affordable), GUI.skin.button);
            GUI.enabled = enabled;
        }

        private string DescribeTitle()
        {
            string text = "연구  (U: 닫기)";
            foreach (ResourceDefinition kind in wallet.Kinds)
            {
                text += $"   {kind.DisplayName} {wallet.GetAmount(kind)}";
            }
            return text;
        }

        // 다른 갈래의 선행 연구는 위아래 순서로 보이지 않으므로 이름을 적는다
        private string DescribeCost(UpgradeDefinition upgrade)
        {
            string text = "비용: " + ResourceAmount.Describe(upgrade.Cost);
            foreach (UpgradeDefinition required in upgrade.Prerequisites)
            {
                if (required == null || required.Branch == upgrade.Branch || research.IsCompleted(required)) continue;

                text += $" · {required.DisplayName} 필요";
            }
            return text;
        }

        private static string DescribeState(ResearchStatus status, bool affordable)
        {
            switch (status)
            {
                case ResearchStatus.Completed: return "완료";
                case ResearchStatus.TowerLocked: return "해금 필요";
                case ResearchStatus.OtherChosen: return "선택 마감";
                case ResearchStatus.MissingPrerequisite: return "선행 필요";
                default: return affordable ? "연구" : "자원 부족";
            }
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
            if (heading != null) return;

            heading = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = false };
            small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, padding = new RectOffset(0, 0, 0, 0) };
            center = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        }
    }
}
