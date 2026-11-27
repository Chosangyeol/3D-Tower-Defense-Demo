using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Core
{
    public enum GameMode
    {
        Build,  // 탑뷰 배치
        Combat  // 1인칭 전투
    }

    /// <summary>
    /// 현재 게임 모드를 관리한다. 다른 컴포넌트는 ModeChanged 이벤트로 반응한다.
    /// 배치 ↔ 전투 전환 주기는 아직 미정이라, 지금은 단축키로만 전환한다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameModeController : MonoBehaviour
    {
        [SerializeField] private GameMode startMode = GameMode.Build;
        [SerializeField] private InputAction toggleAction = new InputAction("ToggleMode", InputActionType.Button, "<Keyboard>/tab");

        public GameMode CurrentMode { get; private set; }

        public event Action<GameMode> ModeChanged;

        private void Awake()
        {
            CurrentMode = startMode;
        }

        private void OnEnable()
        {
            toggleAction.performed += OnToggle;
            toggleAction.Enable();
        }

        private void OnDisable()
        {
            toggleAction.performed -= OnToggle;
            toggleAction.Disable();
        }

        public void SetMode(GameMode mode)
        {
            if (mode == CurrentMode) return;

            CurrentMode = mode;
            ModeChanged?.Invoke(mode);
        }

        private void OnToggle(InputAction.CallbackContext context)
        {
            SetMode(CurrentMode == GameMode.Build ? GameMode.Combat : GameMode.Build);
        }
    }
}
