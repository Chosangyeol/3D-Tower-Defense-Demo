using System;
using TowerDefense.Combat;
using TowerDefense.Controls;
using TowerDefense.Waves;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TowerDefense.Game
{
    public enum MatchResult
    {
        InProgress,
        Victory, // 마지막 웨이브까지 막아냄 (클리어)
        Defeat   // 기지 체력 0
    }

    /// <summary>
    /// 한 판의 승패를 판정하고 기록한다. 끝나면 게임을 멈추고, 지정한 컴포넌트를 꺼서 조작을 막는다.
    /// </summary>
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunConstraints constraints;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private Health baseHealth;
        [SerializeField] private Behaviour[] disableOnEnd = new Behaviour[0];

        public MatchResult Result { get; private set; }

        /// <summary>승패가 정해지고 기록을 남긴 뒤에 호출된다.</summary>
        public event Action<MatchResult> Ended;

        private void OnEnable()
        {
            baseHealth.Died += OnBaseDestroyed;
            waveSpawner.WaveCleared += OnWaveCleared;
            input.Restart.performed += OnRestart;
        }

        private void OnDisable()
        {
            baseHealth.Died -= OnBaseDestroyed;
            waveSpawner.WaveCleared -= OnWaveCleared;
            input.Restart.performed -= OnRestart;
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

        private void OnBaseDestroyed(Health health)
        {
            End(MatchResult.Defeat);
        }

        private void OnWaveCleared(int wave)
        {
            if (waveSpawner.IsAllCleared && baseHealth.IsAlive) End(MatchResult.Victory);
        }

        private void End(MatchResult result)
        {
            if (Result != MatchResult.InProgress) return;

            Result = result;
            RunRecord.Register(setup.Campaign, result == MatchResult.Victory, waveSpawner.StartedWaves, constraints.Score);
            foreach (Behaviour behaviour in disableOnEnd)
            {
                if (behaviour != null) behaviour.enabled = false;
            }
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Ended?.Invoke(result);
        }

        private void OnRestart(InputAction.CallbackContext context)
        {
            // 다시 시작 키는 게임이 끝난 뒤에만 동작한다
            if (Result != MatchResult.InProgress) Restart();
        }
    }
}
