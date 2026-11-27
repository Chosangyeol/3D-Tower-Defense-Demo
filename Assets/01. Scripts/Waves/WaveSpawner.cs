using System;
using System.Collections;
using TowerDefense.Controls;
using TowerDefense.Enemies;
using TowerDefense.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Waves
{
    /// <summary>
    /// 스테이지 데이터의 웨이브를 하나씩 내보낸다. 모두 내보냈고 남은 적이 없으면 웨이브가 끝난다.
    /// 준비 시간이 지나면 다음 웨이브가 자동으로 시작되고, 단축키로 바로 시작할 수도 있다.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private StageController stageController;
        [SerializeField] private EnemyPath path;

        private bool spawning;

        private StageDefinition Stage => stageController.Stage;

        public int WaveCount => Stage.Waves.Count;
        /// <summary>지금까지 시작한 웨이브 수.</summary>
        public int StartedWaves { get; private set; }
        public int AliveCount { get; private set; }
        /// <summary>기지에 도달한 적 수.</summary>
        public int ReachedGoalCount { get; private set; }
        /// <summary>처치한 적 수.</summary>
        public int KilledCount { get; private set; }
        public bool IsWaveRunning { get; private set; }
        public bool IsAllCleared => StartedWaves >= WaveCount && !IsWaveRunning;
        /// <summary>다음 웨이브를 기다리는 준비 시간인지 여부.</summary>
        public bool IsPreparing => !IsWaveRunning && StartedWaves < WaveCount;
        /// <summary>다음 웨이브가 자동으로 시작되기까지 남은 시간(초). 준비 시간이 아니면 0.</summary>
        public float TimeUntilNextWave { get; private set; }

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action<Enemy> EnemyReachedGoal;

        private void Awake()
        {
            TimeUntilNextWave = Stage.FirstWaveDelay;
        }

        private void Update()
        {
            if (!IsPreparing) return;

            TimeUntilNextWave = Mathf.Max(TimeUntilNextWave - Time.deltaTime, 0f);
            if (TimeUntilNextWave <= 0f) StartNextWave();
        }

        private void OnEnable()
        {
            input.StartWave.performed += OnStart;
        }

        private void OnDisable()
        {
            input.StartWave.performed -= OnStart;
        }

        public bool StartNextWave()
        {
            if (IsWaveRunning || StartedWaves >= WaveCount) return false;

            WaveDefinition wave = Stage.Waves[StartedWaves];
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

        private IEnumerator Spawn(WaveDefinition wave)
        {
            if (wave == null) Debug.LogError($"웨이브 {StartedWaves}의 데이터가 비어 있습니다.", this);

            int groupCount = wave != null ? wave.Groups.Count : 0;
            for (int g = 0; g < groupCount; g++)
            {
                SpawnGroup group = wave.Groups[g];
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

            // 적이 하나도 없는 웨이브도 시작한 프레임이 지난 뒤에 끝낸다
            if (AliveCount == 0 && groupCount == 0) yield return null;

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
            if (IsPreparing) TimeUntilNextWave = Stage.WaveInterval;
            WaveCleared?.Invoke(StartedWaves);
        }
    }
}
