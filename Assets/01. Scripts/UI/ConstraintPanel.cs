using System.Collections.Generic;
using System.Linq;
using TowerDefense.Controls;
using TowerDefense.Game;
using TowerDefense.Waves;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.UI
{
    /// <summary>
    /// 프로토타입용 임시 제약 선택 화면. 한 번 클리어하면 열 수 있다.
    /// 판이 끝나면 자동으로 열리고, 첫 웨이브 전에는 제약 키로 연다. 고른 제약은 새로 시작할 때 적용된다.
    /// </summary>
    public class ConstraintPanel : MonoBehaviour
    {
        private const float Width = 480f;
        private const float HeaderHeight = 48f;
        private const float EntryHeight = 52f;
        private const float FooterHeight = 44f;
        private const float ButtonWidth = 90f;

        [SerializeField] private GameInput input;
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunConstraints constraints;
        [SerializeField] private MatchController match;
        [SerializeField] private WaveSpawner waveSpawner;

        private List<ConstraintDefinition> selected = new List<ConstraintDefinition>();
        private bool startPressed;

        public bool IsOpen { get; private set; }

        /// <summary>지금 열 수 있는지 여부.</summary>
        public bool CanOpen =>
            RunRecord.IsConstraintUnlocked(setup.Campaign)
            && (match.Result != MatchResult.InProgress || waveSpawner.StartedWaves == 0);

        private IReadOnlyList<ConstraintDefinition> All => setup.Campaign.Constraints;

        private Rect PanelRect
        {
            get
            {
                float height = HeaderHeight + EntryHeight * All.Count + FooterHeight;
                // 결과 화면과 겹치지 않도록 화면 아래쪽에 놓는다
                float y = match.Result == MatchResult.InProgress
                    ? (Screen.height - height) * 0.5f
                    : Mathf.Max(Screen.height - height - 16f, 0f);
                return new Rect((Screen.width - Width) * 0.5f, y, Width, height);
            }
        }

        private void OnEnable()
        {
            input.Constraints.performed += OnToggle;
            input.Click.performed += OnClick;
            input.Click.canceled += OnRelease;
            match.Ended += OnMatchEnded;
        }

        private void OnDisable()
        {
            input.Constraints.performed -= OnToggle;
            input.Click.performed -= OnClick;
            input.Click.canceled -= OnRelease;
            match.Ended -= OnMatchEnded;
        }

        public void SetOpen(bool open)
        {
            open = open && CanOpen;
            if (open == IsOpen) return;

            IsOpen = open;
            if (open) selected = RunRecord.SelectedConstraints(setup.Campaign);

            // 첫 웨이브 전에 고르는 동안에는 준비 시간이 흐르거나 웨이브가 시작되지 않게 한다
            if (match.Result == MatchResult.InProgress) waveSpawner.enabled = !open;
        }

        /// <summary>항목의 선택 버튼 영역. GUI 좌표(왼쪽 위 기준).</summary>
        public Rect GetToggleRect(int index)
        {
            Rect panel = PanelRect;
            return new Rect(panel.xMax - ButtonWidth - 10f, panel.y + HeaderHeight + EntryHeight * index + 10f, ButtonWidth, 28f);
        }

        /// <summary>새로 시작 버튼 영역. GUI 좌표.</summary>
        public Rect GetStartRect()
        {
            Rect panel = PanelRect;
            return new Rect(panel.center.x - 110f, panel.yMax - 36f, 220f, 28f);
        }

        private void OnMatchEnded(MatchResult result)
        {
            IsOpen = false;
            SetOpen(true);
        }

        private void OnToggle(InputAction.CallbackContext context)
        {
            // 결과 화면에서는 항상 열어 둔다
            if (match.Result == MatchResult.InProgress) SetOpen(!IsOpen);
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            startPressed = false;
            if (!IsOpen) return;

            Vector2 pointer = ReadPointer();
            for (int i = 0; i < All.Count; i++)
            {
                if (!GetToggleRect(i).Contains(pointer)) continue;

                if (!selected.Remove(All[i])) selected.Add(All[i]);
                RunRecord.SaveSelection(setup.Campaign, selected);
                return;
            }
            startPressed = GetStartRect().Contains(pointer);
        }

        // 누른 순간에 새로 시작하면 눌린 클릭이 새 판의 타워 설치로 이어지므로, 손을 뗄 때 시작한다
        private void OnRelease(InputAction.CallbackContext context)
        {
            bool start = startPressed && IsOpen && GetStartRect().Contains(ReadPointer());
            startPressed = false;
            if (start) match.Restart();
        }

        // GUI 좌표(왼쪽 위 기준)의 마우스 위치
        private Vector2 ReadPointer()
        {
            Vector2 screen = input.Point.ReadValue<Vector2>();
            return new Vector2(screen.x, Screen.height - screen.y);
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            Rect panel = PanelRect;
            GUI.Box(panel, GUIContent.none);
            int score = selected.Sum(c => c.Score);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 4f, panel.width - 20f, 20f),
                $"제약 선택 · 난이도 {score} (지금 판 {constraints.Score})");
            GUI.Label(new Rect(panel.x + 10f, panel.y + 24f, panel.width - 20f, 20f),
                match.Result == MatchResult.InProgress ? "새로 시작해야 적용됩니다 · C: 닫기" : "고른 제약은 다음 판에 적용됩니다");

            for (int i = 0; i < All.Count; i++)
            {
                ConstraintDefinition constraint = All[i];
                if (constraint == null) continue;

                var line = new Rect(panel.x + 10f, panel.y + HeaderHeight + EntryHeight * i + 4f, panel.width - ButtonWidth - 30f, 20f);
                GUI.Label(line, $"{constraint.DisplayName}  (+{constraint.Score})");
                line.y += 20f;
                GUI.Label(line, constraint.Description);
                GUI.Box(GetToggleRect(i), selected.Contains(constraint) ? "선택됨 ✔" : "선택", GUI.skin.button);
            }
            GUI.Box(GetStartRect(), "이 제약으로 새로 시작", GUI.skin.button);
        }
    }
}
