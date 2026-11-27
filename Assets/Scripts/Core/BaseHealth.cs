using System;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// 지켜야 할 기지의 체력. 적이 도달할 때마다 줄고, 0이 되면 파괴된다.
    /// </summary>
    public class BaseHealth : MonoBehaviour
    {
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField, Min(1)] private int maxHealth = 10;
        [SerializeField, Min(1)] private int damagePerEnemy = 1;

        public int Max => maxHealth;
        public int Current { get; private set; }

        /// <summary>체력이 0이 되는 순간 한 번 호출된다.</summary>
        public event Action Destroyed;

        private void Awake()
        {
            Current = maxHealth;
        }

        private void OnEnable()
        {
            waveSpawner.EnemyReachedGoal += OnEnemyReachedGoal;
        }

        private void OnDisable()
        {
            waveSpawner.EnemyReachedGoal -= OnEnemyReachedGoal;
        }

        public void TakeDamage(int amount)
        {
            if (Current <= 0 || amount <= 0) return;

            Current = Mathf.Max(Current - amount, 0);
            if (Current == 0) Destroyed?.Invoke();
        }

        private void OnEnemyReachedGoal(Enemy enemy)
        {
            TakeDamage(damagePerEnemy);
        }
    }
}
