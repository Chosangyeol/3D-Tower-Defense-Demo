using TowerDefense.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Controls
{
    /// <summary>
    /// 게임의 모든 입력이 지나는 곳. 다른 컴포넌트는 키를 직접 읽지 않고 여기의 액션을 사용한다.
    /// 키 설정은 GameControls.inputactions에 있고, 게임 모드에 따라 배치·전투 액션을 켜고 끈다.
    /// 설정 화면에서 바꾼 키는 SaveBindings로 저장되며 다음 실행 때 다시 적용된다.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class GameInput : MonoBehaviour
    {
        private const string BindingsKey = "input.bindingOverrides";
        public const int SlotCount = 9;

        [SerializeField] private InputActionAsset actions;
        [SerializeField] private GameModeController modeController;

        private InputActionMap globalMap;
        private InputActionMap buildMap;
        private InputActionMap combatMap;
        private readonly InputAction[] slots = new InputAction[SlotCount];

        /// <summary>키 설정 화면에서 바인딩을 바꿀 때 사용한다.</summary>
        public InputActionAsset Actions => actions;

        // 공통
        public InputAction ToggleMode { get; private set; }
        public InputAction StartWave { get; private set; }
        public InputAction Restart { get; private set; }
        public InputAction Point { get; private set; }
        public InputAction Click { get; private set; }
        public InputAction Constraints { get; private set; }

        // 배치 모드
        public InputAction Pan { get; private set; }
        public InputAction Rotate { get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Place { get; private set; }
        public InputAction Remove { get; private set; }
        public InputAction Research { get; private set; }

        // 전투 모드
        public InputAction Move { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Jump { get; private set; }
        public InputAction Sprint { get; private set; }
        public InputAction Cast { get; private set; }
        public InputAction Interact { get; private set; }

        /// <summary>타워 선택 칸. index는 0부터 SlotCount - 1까지.</summary>
        public InputAction Slot(int index)
        {
            return slots[index];
        }

        private void Awake()
        {
            globalMap = actions.FindActionMap("Global", true);
            buildMap = actions.FindActionMap("Build", true);
            combatMap = actions.FindActionMap("Combat", true);

            ToggleMode = globalMap.FindAction("ToggleMode", true);
            StartWave = globalMap.FindAction("StartWave", true);
            Restart = globalMap.FindAction("Restart", true);
            Point = globalMap.FindAction("Point", true);
            Click = globalMap.FindAction("Click", true);
            Constraints = globalMap.FindAction("Constraints", true);

            Pan = buildMap.FindAction("Pan", true);
            Rotate = buildMap.FindAction("Rotate", true);
            Zoom = buildMap.FindAction("Zoom", true);
            Place = buildMap.FindAction("Place", true);
            Remove = buildMap.FindAction("Remove", true);
            Research = buildMap.FindAction("Research", true);
            for (int i = 0; i < SlotCount; i++)
            {
                slots[i] = buildMap.FindAction($"Slot{i + 1}", true);
            }

            Move = combatMap.FindAction("Move", true);
            Look = combatMap.FindAction("Look", true);
            Jump = combatMap.FindAction("Jump", true);
            Sprint = combatMap.FindAction("Sprint", true);
            Cast = combatMap.FindAction("Cast", true);
            Interact = combatMap.FindAction("Interact", true);

            LoadBindings();
        }

        private void OnEnable()
        {
            globalMap.Enable();
            modeController.ModeChanged += ApplyMode;
        }

        private void Start()
        {
            ApplyMode(modeController.CurrentMode);
        }

        private void OnDisable()
        {
            modeController.ModeChanged -= ApplyMode;
            globalMap.Disable();
            buildMap.Disable();
            combatMap.Disable();
        }

        /// <summary>지금의 키 설정을 저장한다.</summary>
        public void SaveBindings()
        {
            PlayerPrefs.SetString(BindingsKey, actions.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        }

        /// <summary>저장한 키 설정을 적용한다. 저장한 것이 없으면 기본 키를 쓴다.</summary>
        public void LoadBindings()
        {
            actions.RemoveAllBindingOverrides();
            string json = PlayerPrefs.GetString(BindingsKey, string.Empty);
            if (!string.IsNullOrEmpty(json)) actions.LoadBindingOverridesFromJson(json);
        }

        /// <summary>키 설정을 기본값으로 되돌리고 저장한 내용을 지운다.</summary>
        public void ResetBindings()
        {
            actions.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(BindingsKey);
            PlayerPrefs.Save();
        }

        private void ApplyMode(GameMode mode)
        {
            bool build = mode == GameMode.Build;
            if (build)
            {
                combatMap.Disable();
                buildMap.Enable();
            }
            else
            {
                buildMap.Disable();
                combatMap.Enable();
            }
        }
    }
}
