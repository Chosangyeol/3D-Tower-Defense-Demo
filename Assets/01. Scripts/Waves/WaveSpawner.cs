using System;
using System.Collections;
using TowerDefense.Combat;
using TowerDefense.Controls;
using TowerDefense.Effects;
using TowerDefense.Enemies;
using TowerDefense.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Waves
{
    /// <summary>
    /// 진행 데이터의 웨이브를 하나씩 내보낸다. 모두 내보냈고 부하를 포함한 적을 모두 처치하면 웨이브가 끝난다.
    /// 준비 시간이 지나면 다음 웨이브가 자동으로 시작되고, 단축키로 바로 시작할 수도 있다.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunModifiers modifiers;
        [SerializeField] private EnemyPath path;
        [Tooltip("적이 도착해서 공격할 기지")]
        [SerializeField] private Health goal;

        private bool spawning;
        private WaveScale scale;

        private CampaignDefinition Campaign => setup.Campaign;

        public int WaveCount => Campaign.Waves.Count;
        /// <summary>지금까지 시작한 웨이브 수.</summary>
        public int StartedWaves { get; private set; }
        public int AliveCount { get; private set; }
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
        public event Action<Enemy> EnemyKilled;

        private void Awake()
        {
            TimeUntilNextWave = modifiers.Get(Stat.PrepSeconds, Campaign.FirstWaveDelay);
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

            WaveDefinition wave = Campaign.Waves[StartedWaves];
            StartedWaves++;
            scale = Campaign.GetScale(StartedWaves);
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

                int count = GetCount(group);
                for (int i = 0; i < count; i++)
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

        // 보스의 수는 배율과 효과로 늘리지 않는다
        private int GetCount(SpawnGroup group)
        {
            return group.enemy.IsBoss
                ? group.count
                : modifiers.GetInt(Stat.EnemyCount, group.count * scale.Count, group.enemy);
        }

        private EnemyStats CreateStats(EnemyDefinition definition)
        {
            return new EnemyStats(
                modifiers.Get(Stat.EnemyHealth, definition.MaxHealth * scale.Health, definition),
                modifiers.Get(Stat.EnemyMoveSpeed, definition.MoveSpeed, definition),
                modifiers.GetInt(Stat.EnemyAttackDamage, definition.AttackDamage * scale.Attack, definition),
                scale.Reward);
        }

        private void SpawnEnemy(EnemyDefinition definition, float startDistance = 0f)
        {
            Enemy enemy = Instantiate(definition.Prefab, transform);
            enemy.name = definition.name;
            enemy.Initialize(definition, CreateStats(definition), path, goal, startDistance);
            enemy.Finished += OnEnemyFinished;
            enemy.SummonRequested += OnSummonRequested;
            AliveCount++;
        }

        // 부하는 부른 적의 조금 뒤에 한 마리씩 간격을 두고 나타난다
        private void OnSummonRequested(Enemy summoner, EnemyDefinition definition, int count)
        {
            if (definition.Prefab == null) return;

            for (int i = 0; i < count; i++)
            {
                float distance = Mathf.Max(summoner.DistanceTravelled - 1f - 0.8f * i, 0f);
                SpawnEnemy(definition, distance);
            }
        }

        private void OnEnemyFinished(Enemy enemy, EnemyOutcome outcome)
        {
            enemy.Finished -= OnEnemyFinished;
            enemy.SummonRequested -= OnSummonRequested;
            AliveCount--;
            if (outcome == EnemyOutcome.Killed)
            {
                KilledCount++;
                EnemyKilled?.Invoke(enemy);
            }
            CheckCleared();
        }

        private void CheckCleared()
        {
            if (!IsWaveRunning || spawning || AliveCount > 0) return;

            IsWaveRunning = false;
            if (IsPreparing) TimeUntilNextWave = modifiers.Get(Stat.PrepSeconds, Campaign.WaveInterval);
            WaveCleared?.Invoke(StartedWaves);
        }
    }
}
