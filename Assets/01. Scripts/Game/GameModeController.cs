using System;
using TowerDefense.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Game
{
    public enum GameMode
    {
        Build,  // 탑뷰 배치
        Combat  // 1인칭 전투
    }

    /// <summary>
    /// 현재 게임 모드를 관리한다. 다른 컴포넌트는 ModeChanged 이벤트로 반응한다.
    /// 시점은 웨이브의 시작·종료와 관계없이 플레이어가 단축키로 직접 전환한다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameModeController : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameMode startMode = GameMode.Build;

        public GameMode CurrentMode { get; private set; }

        public event Action<GameMode> ModeChanged;

        private void Awake()
        {
            CurrentMode = startMode;
        }

        private void OnEnable()
        {
            input.ToggleMode.performed += OnToggle;
        }

        private void OnDisable()
        {
            input.ToggleMode.performed -= OnToggle;
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
