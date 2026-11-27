using System;
using System.Collections.Generic;
using System.Linq;
using TowerDefense.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.EditorTools
{
    /// <summary>
    /// 게임 데이터 관리 창. 종류 선택 → 목록 → 편집의 3단 구성.
    /// 메뉴: Tools > Tower Defense > Data Manager
    /// </summary>
    public class DataManagerWindow : EditorWindow
    {
        private const int MaxListedReferences = 10;

        // 창을 다시 열거나 스크립트를 다시 컴파일해도 선택이 유지되도록 저장한다
        [SerializeField] private string selectedCategoryType;
        [SerializeField] private GameData selected;
        [SerializeField] private string searchText = string.Empty;

        private List<GameDataCategory> categories = new List<GameDataCategory>();
        private readonly List<GameData> allData = new List<GameData>();
        private readonly List<GameData> visibleData = new List<GameData>();

        private ListView categoryList;
        private ListView dataList;
        private VisualElement detail;
        private Button createButton;
        private Button duplicateButton;
        private Button deleteButton;

        private GameDataCategory Category =>
            categories.FirstOrDefault(c => c.Type.FullName == selectedCategoryType);

        [MenuItem("Tools/Tower Defense/Data Manager")]
        public static void Open()
        {
            var window = GetWindow<DataManagerWindow>();
            window.titleContent = new GUIContent("데이터 관리");
            window.minSize = new Vector2(720f, 360f);
        }

        private void OnEnable()
        {
            EditorApplication.projectChanged += Reload;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= Reload;
        }

        private void CreateGUI()
        {
            var toolbar = new Toolbar();
            toolbar.Add(new ToolbarButton(Reload) { text = "새로고침" });
            toolbar.Add(new ToolbarButton(ExportCsv) { text = "CSV 내보내기" });
            toolbar.Add(new ToolbarButton(ImportCsv) { text = "CSV 가져오기" });
            toolbar.Add(new ToolbarSpacer { style = { flexGrow = 1f } });
            toolbar.Add(new ToolbarButton(AssetDatabase.SaveAssets) { text = "저장" });
            rootVisualElement.Add(toolbar);

            var outer = new TwoPaneSplitView(0, 140f, TwoPaneSplitViewOrientation.Horizontal);
            var inner = new TwoPaneSplitView(0, 250f, TwoPaneSplitViewOrientation.Horizontal);
            outer.Add(CreateCategoryPane());
            outer.Add(inner);
            inner.Add(CreateDataPane());
            detail = new VisualElement { style = { paddingLeft = 8f, paddingRight = 8f, paddingTop = 4f } };
            inner.Add(detail);
            rootVisualElement.Add(outer);

            Reload();
        }

        // 선택한 종류의 데이터를 CSV로 저장한다
        private void ExportCsv()
        {
            GameDataCategory category = Category;
            if (category == null) return;

            string path = EditorUtility.SaveFilePanel("CSV 내보내기", GetCsvFolder(), category.Folder, "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                DataCsvService.ExportToFile(category, path);
                ShowNotification(new GUIContent("내보냈습니다"));
            }
            catch (System.IO.IOException e)
            {
                EditorUtility.DisplayDialog("내보내기 실패", "파일이 다른 프로그램에서 열려 있는지 확인하세요.\n" + e.Message, "확인");
            }
        }

        // 선택한 종류의 CSV를 읽어 미리보기 창을 연다
        private void ImportCsv()
        {
            GameDataCategory category = Category;
            if (category == null) return;

            string path = EditorUtility.OpenFilePanel($"CSV 가져오기 ({category.Label})", GetCsvFolder(), "csv");
            if (string.IsNullOrEmpty(path)) return;

            CsvImportWindow.Show(DataCsvService.PlanFromFile(category, path));
        }

        // 프로젝트 폴더의 DataCSV를 기본 위치로 쓴다
        private static string GetCsvFolder()
        {
            string folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "DataCSV"));
            System.IO.Directory.CreateDirectory(folder);
            return folder;
        }

        private VisualElement CreateCategoryPane()
        {
            categoryList = new ListView
            {
                fixedItemHeight = 24f,
                selectionType = SelectionType.Single,
                makeItem = MakeRow,
                bindItem = (element, index) => ((Label)element).text = categories[index].Label
            };
            categoryList.selectionChanged += _ =>
            {
                int index = categoryList.selectedIndex;
                if (index < 0 || index >= categories.Count) return;

                selectedCategoryType = categories[index].Type.FullName;
                ReloadData();
            };
            return categoryList;
        }

        private VisualElement CreateDataPane()
        {
            var pane = new VisualElement();

            var search = new ToolbarSearchField { value = searchText, style = { width = StyleKeyword.Auto } };
            search.RegisterValueChangedCallback(change =>
            {
                searchText = change.newValue;
                ApplyFilter();
            });
            pane.Add(search);

            dataList = new ListView
            {
                fixedItemHeight = 22f,
                selectionType = SelectionType.Single,
                makeItem = MakeRow,
                bindItem = (element, index) =>
                {
                    GameData data = visibleData[index];
                    ((Label)element).text = data == null ? "(삭제됨)" : $"{data.DisplayName}  ({data.name})";
                },
                style = { flexGrow = 1f }
            };
            dataList.selectionChanged += _ =>
            {
                int index = dataList.selectedIndex;
                if (index >= 0 && index < visibleData.Count) Select(visibleData[index]);
            };
            pane.Add(dataList);

            var buttons = new VisualElement { style = { flexDirection = FlexDirection.Row, flexShrink = 0f } };
            createButton = new Button(CreateData) { text = "새로 만들기", style = { flexGrow = 1f } };
            duplicateButton = new Button(DuplicateData) { text = "복제", style = { flexGrow = 1f } };
            deleteButton = new Button(DeleteData) { text = "삭제", style = { flexGrow = 1f } };
            buttons.Add(createButton);
            buttons.Add(duplicateButton);
            buttons.Add(deleteButton);
            pane.Add(buttons);
            return pane;
        }

        private static VisualElement MakeRow()
        {
            return new Label { style = { paddingLeft = 6f, unityTextAlign = TextAnchor.MiddleLeft } };
        }

        /// <summary>종류와 데이터 목록을 처음부터 다시 읽는다.</summary>
        private void Reload()
        {
            // 창이 아직 만들어지기 전에 호출될 수 있다
            if (categoryList == null) return;

            categories = GameDataRepository.GetCategories();
            if (Category == null && categories.Count > 0) selectedCategoryType = categories[0].Type.FullName;

            categoryList.itemsSource = categories;
            categoryList.Rebuild();
            categoryList.SetSelectionWithoutNotify(new[] { categories.IndexOf(Category) });
            ReloadData();
        }

        private void ReloadData()
        {
            allData.Clear();
            if (Category != null) allData.AddRange(GameDataRepository.FindAll(Category));

            // 지워졌거나 다른 종류의 데이터는 선택을 푼다
            if (!allData.Contains(selected)) selected = null;
            ApplyFilter();
            ShowDetail();
        }

        private void ApplyFilter()
        {
            visibleData.Clear();
            visibleData.AddRange(allData.Where(data => Matches(data, searchText)));

            dataList.itemsSource = visibleData;
            dataList.Rebuild();
            int index = visibleData.IndexOf(selected);
            if (index >= 0) dataList.SetSelectionWithoutNotify(new[] { index });
            else dataList.ClearSelection();
            UpdateButtons();
        }

        private static bool Matches(GameData data, string text)
        {
            return string.IsNullOrEmpty(text)
                   || data.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0
                   || data.DisplayName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Select(GameData data)
        {
            if (selected == data && detail.childCount > 0) return;

            selected = data;
            ShowDetail();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            createButton.SetEnabled(Category != null);
            duplicateButton.SetEnabled(selected != null);
            deleteButton.SetEnabled(selected != null);
        }

        private void ShowDetail()
        {
            detail.Clear();
            if (selected == null)
            {
                detail.Add(new HelpBox("왼쪽 목록에서 데이터를 선택하거나 새로 만드세요.", HelpBoxMessageType.Info));
                return;
            }

            GameData data = selected;
            var header = new VisualElement { style = { flexDirection = FlexDirection.Row, flexShrink = 0f, marginBottom = 6f } };
            var nameField = new TextField("에셋 이름") { value = data.name, style = { flexGrow = 1f } };
            header.Add(nameField);
            header.Add(new Button(() => RenameData(data, nameField.value)) { text = "이름 변경" });
            header.Add(new Button(() => EditorGUIUtility.PingObject(data)) { text = "프로젝트에서 보기" });
            detail.Add(header);

            var scroll = new ScrollView { style = { flexGrow = 1f } };
            var serialized = new SerializedObject(data);
            // 인스펙터와 같은 화면이라 실행 취소(Ctrl+Z)도 동작한다
            var inspector = new InspectorElement(serialized);
            // 표시 이름이 바뀌면 목록도 갱신한다
            inspector.TrackSerializedObjectValue(serialized, _ => dataList.RefreshItems());
            scroll.Add(inspector);
            detail.Add(scroll);
        }

        private void CreateData()
        {
            GameDataCategory category = Category;
            if (category == null) return;

            if (category.CreatableTypes.Count == 1)
            {
                CreateData(category, category.CreatableTypes[0]);
                return;
            }

            // 하위 클래스가 여러 개면 만들 종류를 고른다
            var menu = new GenericMenu();
            foreach (Type type in category.CreatableTypes)
            {
                Type target = type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(target.Name)), false, () => CreateData(category, target));
            }
            menu.ShowAsContext();
        }

        private void CreateData(GameDataCategory category, Type type)
        {
            ShowCreated(GameDataRepository.Create(category, type, "New" + type.Name));
        }

        private void DuplicateData()
        {
            if (selected != null) ShowCreated(GameDataRepository.Duplicate(selected));
        }

        private void ShowCreated(GameData data)
        {
            if (data == null) return;

            // 새 항목이 검색어에 가려지지 않게 한다
            searchText = string.Empty;
            rootVisualElement.Q<ToolbarSearchField>().SetValueWithoutNotify(searchText);
            selected = data;
            ReloadData();
            dataList.ScrollToItem(visibleData.IndexOf(data));
        }

        private void RenameData(GameData data, string newName)
        {
            string error = GameDataRepository.Rename(data, newName);
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("이름 변경 실패", error, "확인");
                return;
            }
            ReloadData();
        }

        private void DeleteData()
        {
            GameData data = selected;
            if (data == null) return;

            List<string> references = GameDataRepository.FindReferences(data);
            string message = $"'{data.DisplayName}' ({data.name})을 삭제할까요?\n파일은 휴지통으로 이동합니다.";
            if (references.Count > 0)
            {
                message += $"\n\n주의: {references.Count}곳에서 사용 중입니다. 삭제하면 참조가 비게 됩니다.\n"
                           + string.Join("\n", references.Take(MaxListedReferences));
                if (references.Count > MaxListedReferences) message += "\n…";
            }

            if (!EditorUtility.DisplayDialog("데이터 삭제", message, "삭제", "취소")) return;

            selected = null;
            GameDataRepository.Delete(data);
            ReloadData();
        }
    }
}
