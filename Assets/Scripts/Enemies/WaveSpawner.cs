using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Enemies
{
    /// <summary>같은 종류의 적을 일정 간격으로 내보내는 묶음.</summary>
    [Serializable]
    public struct SpawnGroup
    {
        public EnemyDefinition enemy;
        [Min(1)] public int count;
        [Min(0.05f)] public float interval;
    }

    /// <summary>웨이브 하나. 묶음을 순서대로 내보낸다.</summary>
    [Serializable]
    public struct Wave
    {
        public SpawnGroup[] groups;
    }

    /// <summary>
    /// 웨이브를 하나씩 내보낸다. 모두 내보냈고 남은 적이 없으면 웨이브가 끝난다.
    /// 준비 시간이 지나면 다음 웨이브가 자동으로 시작되고, 단축키로 바로 시작할 수도 있다.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private EnemyPath path;
        [SerializeField] private Wave[] waves = new Wave[0];
        [SerializeField] private InputAction startAction = new InputAction("StartWave", InputActionType.Button, "<Keyboard>/enter");

        [Header("준비 시간(초) - 플레이 테스트 후 조정")]
        [Tooltip("게임 시작부터 첫 웨이브까지. 마을·타워 배치를 고려해 넉넉하게 둔다")]
        [SerializeField, Min(1f)] private float firstWaveDelay = 180f;
        [Tooltip("웨이브가 끝난 뒤 다음 웨이브까지")]
        [SerializeField, Min(1f)] private float waveInterval = 60f;

        private bool spawning;

        public int WaveCount => waves.Length;
        /// <summary>지금까지 시작한 웨이브 수.</summary>
        public int StartedWaves { get; private set; }
        public int AliveCount { get; private set; }
        /// <summary>기지에 도달한 적 수.</summary>
        public int ReachedGoalCount { get; private set; }
        /// <summary>처치한 적 수.</summary>
        public int KilledCount { get; private set; }
        public bool IsWaveRunning { get; private set; }
        public bool IsAllCleared => StartedWaves >= waves.Length && !IsWaveRunning;
        /// <summary>다음 웨이브를 기다리는 준비 시간인지 여부.</summary>
        public bool IsPreparing => !IsWaveRunning && StartedWaves < waves.Length;
        /// <summary>다음 웨이브가 자동으로 시작되기까지 남은 시간(초). 준비 시간이 아니면 0.</summary>
        public float TimeUntilNextWave { get; private set; }

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action<Enemy> EnemyReachedGoal;

        private void Awake()
        {
            TimeUntilNextWave = firstWaveDelay;
        }

        private void Update()
        {
            if (!IsPreparing) return;

            TimeUntilNextWave = Mathf.Max(TimeUntilNextWave - Time.deltaTime, 0f);
            if (TimeUntilNextWave <= 0f) StartNextWave();
        }

        private void OnEnable()
        {
            startAction.performed += OnStart;
            startAction.Enable();
        }

        private void OnDisable()
        {
            startAction.performed -= OnStart;
            startAction.Disable();
        }

        public bool StartNextWave()
        {
            if (IsWaveRunning || StartedWaves >= waves.Length) return false;

            Wave wave = waves[StartedWaves];
            StartedWaves++;
            IsWaveRunning = true;
            spawning = true;
            TimeUntilNextWave = 0f;
            WaveStarted?.Invoke(StartedWaves);
            StartCoroutine(Spawn(wave));
            return true;
        }

        private void OnStart(InputAction.CallbackContext context)
        {
            StartNextWave();
        }

        private IEnumerator Spawn(Wave wave)
        {
            foreach (SpawnGroup group in wave.groups)
            {
                if (group.enemy == null || group.enemy.Prefab == null)
                {
                    Debug.LogError($"웨이브 {StartedWaves}에 적 데이터가 비어 있습니다.", this);
                    continue;
                }

                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group.enemy);
                    yield return new WaitForSeconds(group.interval);
                }
            }

            spawning = false;
            CheckCleared();
        }

        private void SpawnEnemy(EnemyDefinition definition)
        {
            Enemy enemy = Instantiate(definition.Prefab, transform);
            enemy.name = definition.name;
            enemy.Initialize(definition, path);
            enemy.Finished += OnEnemyFinished;
            AliveCount++;
        }

        private void OnEnemyFinished(Enemy enemy, EnemyOutcome outcome)
        {
            enemy.Finished -= OnEnemyFinished;
            AliveCount--;
            if (outcome == EnemyOutcome.Killed)
            {
                KilledCount++;
            }
            else if (outcome == EnemyOutcome.ReachedGoal)
            {
                ReachedGoalCount++;
                EnemyReachedGoal?.Invoke(enemy);
            }
            CheckCleared();
        }

        private void CheckCleared()
        {
            if (!IsWaveRunning || spawning || AliveCount > 0) return;

            IsWaveRunning = false;
            if (IsPreparing) TimeUntilNextWave = waveInterval;
            WaveCleared?.Invoke(StartedWaves);
        }
    }
}
