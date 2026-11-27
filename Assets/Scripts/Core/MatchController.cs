using TowerDefense.Enemies;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TowerDefense.Core
{
    public enum MatchResult
    {
        InProgress,
        Victory, // 모든 웨이브를 막아냄
        Defeat   // 기지 체력 0
    }

    /// <summary>
    /// 한 판의 승패를 판정한다. 끝나면 게임을 멈추고, 지정한 컴포넌트를 꺼서 조작을 막는다.
    /// </summary>
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private Behaviour[] disableOnEnd = new Behaviour[0];
        [SerializeField] private InputAction restartAction = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");

        public MatchResult Result { get; private set; }

        private void OnEnable()
        {
            baseHealth.Destroyed += OnBaseDestroyed;
            waveSpawner.WaveCleared += OnWaveCleared;
            restartAction.performed += OnRestart;
        }

        private void OnDisable()
        {
            baseHealth.Destroyed -= OnBaseDestroyed;
            waveSpawner.WaveCleared -= OnWaveCleared;
            restartAction.performed -= OnRestart;
            restartAction.Disable();
        }

        private void OnDestroy()
        {
            // 멈춘 상태가 다음 씬이나 다음 플레이에 남지 않게 한다
            Time.timeScale = 1f;
        }

        /// <summary>현재 씬을 처음부터 다시 불러온다.</summary>
        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }

        private void OnBaseDestroyed()
        {
            End(MatchResult.Defeat);
        }

        private void OnWaveCleared(int wave)
        {
            if (waveSpawner.IsAllCleared && baseHealth.Current > 0) End(MatchResult.Victory);
        }

        private void End(MatchResult result)
        {
            if (Result != MatchResult.InProgress) return;

            Result = result;
            foreach (Behaviour behaviour in disableOnEnd)
            {
                if (behaviour != null) behaviour.enabled = false;
            }
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            restartAction.Enable();
        }

        private void OnRestart(InputAction.CallbackContext context)
        {
            Restart();
        }
    }
}
