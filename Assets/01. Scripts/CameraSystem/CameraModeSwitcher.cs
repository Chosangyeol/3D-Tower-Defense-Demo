using TowerDefense.Game;
using Unity.Cinemachine;
using UnityEngine;

namespace TowerDefense.CameraSystem
{
    /// <summary>
    /// 모드에 따라 Cinemachine 카메라의 우선순위와 커서 상태를 바꾼다. 게임이 멈춘 동안에는 커서를 푼다.
    /// 전환 블렌드는 CinemachineBrain의 Default Blend 설정을 따른다.
    /// </summary>
    public class CameraModeSwitcher : MonoBehaviour
    {
        private const int ActivePriority = 20;
        private const int InactivePriority = 0;

        [SerializeField] private GameModeController modeController;
        [SerializeField] private CinemachineCamera topDownCamera;
        [SerializeField] private CinemachineCamera firstPersonCamera;
        [SerializeField] private GamePause pause;

        private void OnEnable()
        {
            modeController.ModeChanged += Apply;
            pause.Changed += OnPauseChanged;
        }

        private void Start()
        {
            Apply(modeController.CurrentMode);
        }

        private void OnDisable()
        {
            modeController.ModeChanged -= Apply;
            pause.Changed -= OnPauseChanged;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // 게임이 끝나 이 컴포넌트가 꺼진 뒤에는 커서를 다시 잠그지 않는다
            if (hasFocus && isActiveAndEnabled && modeController != null) ApplyCursor(modeController.CurrentMode);
        }

        private void Apply(GameMode mode)
        {
            bool build = mode == GameMode.Build;
            topDownCamera.Priority = build ? ActivePriority : InactivePriority;
            firstPersonCamera.Priority = build ? InactivePriority : ActivePriority;
            ApplyCursor(mode);
        }

        private void OnPauseChanged(bool paused)
        {
            ApplyCursor(modeController.CurrentMode);
        }

        private void ApplyCursor(GameMode mode)
        {
            bool combat = mode == GameMode.Combat && !pause.IsPaused;
            Cursor.lockState = combat ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !combat;
        }
    }
}
