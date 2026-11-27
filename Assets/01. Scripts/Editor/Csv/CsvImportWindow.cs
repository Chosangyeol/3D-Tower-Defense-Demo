using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.EditorTools
{
    /// <summary>가져오기 전에 바뀌는 내용을 보여주고, 확인하면 적용한다.</summary>
    public class CsvImportWindow : EditorWindow
    {
        private CsvImportPlan plan;

        public static void Show(CsvImportPlan importPlan)
        {
            var window = GetWindow<CsvImportWindow>(true, "CSV 가져오기 미리보기");
            window.minSize = new Vector2(560f, 360f);
            window.plan = importPlan;
            window.Build();
        }

        private void CreateGUI()
        {
            Build();
        }

        private void Build()
        {
            rootVisualElement.Clear();
            // 스크립트가 다시 컴파일되면 계획이 사라진다
            if (plan == null)
            {
                rootVisualElement.Add(new HelpBox("데이터 관리 창에서 CSV를 다시 가져오세요.", HelpBoxMessageType.Info));
                return;
            }

            int created = plan.Entries.FindAll(e => e.IsNew).Count;
            string summary = $"{plan.Category.Label}: 새로 만들기 {created}개 · 수정 {plan.Entries.Count - created}개 · 오류 {plan.Errors.Count}개";
            rootVisualElement.Add(new Label(summary) { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 6f, marginBottom = 6f, marginLeft = 6f } });

            var scroll = new ScrollView { style = { flexGrow = 1f } };
            if (plan.Errors.Count > 0)
            {
                scroll.Add(new HelpBox("오류를 고친 뒤 다시 가져오세요. 아무것도 바뀌지 않습니다.\n\n" + string.Join("\n", plan.Errors),
                    HelpBoxMessageType.Error));
            }
            else if (plan.Entries.Count == 0)
            {
                scroll.Add(new HelpBox("바뀌는 내용이 없습니다.", HelpBoxMessageType.Info));
            }

            foreach (CsvEntry entry in plan.Entries)
            {
                scroll.Add(CreateEntryView(entry));
            }

            if (plan.NotInCsv.Count > 0)
            {
                scroll.Add(new HelpBox("CSV에 없는 데이터는 삭제하지 않고 그대로 둡니다: " + string.Join(", ", plan.NotInCsv),
                    HelpBoxMessageType.Warning));
            }
            rootVisualElement.Add(scroll);

            var buttons = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, flexShrink = 0f } };
            var apply = new Button(ApplyPlan) { text = "적용" };
            apply.SetEnabled(plan.CanApply);
            buttons.Add(apply);
            buttons.Add(new Button(Close) { text = "취소" });
            rootVisualElement.Add(buttons);
        }

        private static VisualElement CreateEntryView(CsvEntry entry)
        {
            var foldout = new Foldout
            {
                text = $"{(entry.IsNew ? "[새로 만들기]" : "[수정]")} {entry.AssetName} ({entry.Changes.Count})",
                value = true
            };
            var lines = new List<string>();
            foreach (CsvChange change in entry.Changes)
            {
                lines.Add(entry.IsNew ? $"{change.Field}: {change.After}" : $"{change.Field}: {change.Before} → {change.After}");
            }
            foreach (string line in lines)
            {
                foldout.Add(new Label(line) { style = { whiteSpace = WhiteSpace.Normal } });
            }
            return foldout;
        }

        private void ApplyPlan()
        {
            CsvImportPlan current = plan;
            plan = null;
            DataCsvService.Apply(current);
            Close();
        }
    }
}
