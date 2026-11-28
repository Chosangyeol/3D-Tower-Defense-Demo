using TowerDefense.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Game
{
    /// <summary>
    /// 탑뷰에서만 게임을 2배속으로 돌린다. 전투 모드에서는 1배속이 되고, 고른 배속은 탑뷰로 돌아오면 다시 적용된다.
    /// 선택 화면으로 멈추거나 판이 끝난 동안에는 시간을 건드리지 않는다.
    /// </summary>
    public class GameSpeed : MonoBehaviour
    {
        private const float FastScale = 2f;

        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private GamePause pause;
        [SerializeField] private MatchController match;

        /// <summary>탑뷰에서 2배속을 쓰도록 골랐는지 여부.</summary>
        public bool IsFast { get; private set; }

        private bool CanChangeTime => !pause.IsPaused && match.Result == MatchResult.InProgress;

        /// <summary>1배속과 2배속을 바꾼다. 탑뷰에서만 바꿀 수 있다.</summary>
        public bool Toggle()
        {
            if (modeController.CurrentMode != GameMode.Build || !CanChangeTime) return false;

            IsFast = !IsFast;
            Apply();
            return true;
        }

        private void OnEnable()
        {
            input.ToggleSpeed.performed += OnToggle;
            modeController.ModeChanged += OnModeChanged;
        }

        private void OnDisable()
        {
            input.ToggleSpeed.performed -= OnToggle;
            modeController.ModeChanged -= OnModeChanged;
        }

        private void OnToggle(InputAction.CallbackContext context)
        {
            Toggle();
        }

        private void OnModeChanged(GameMode mode)
        {
            Apply();
        }

        private void Apply()
        {
            if (!CanChangeTime) return;

            Time.timeScale = IsFast && modeController.CurrentMode == GameMode.Build ? FastScale : 1f;
        }
    }
}
