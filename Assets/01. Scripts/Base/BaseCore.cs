using TowerDefense.Combat;
using TowerDefense.Enemies;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Base
{
    /// <summary>
    /// 지켜야 할 기지. 적이 도달할 때마다 체력이 줄어든다. 체력은 같은 오브젝트의 Health가 관리한다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class BaseCore : MonoBehaviour
    {
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField, Min(1)] private int damagePerEnemy = 1;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            waveSpawner.EnemyReachedGoal += OnEnemyReachedGoal;
        }

        private void OnDisable()
        {
            waveSpawner.EnemyReachedGoal -= OnEnemyReachedGoal;
        }

        private void OnEnemyReachedGoal(Enemy enemy)
        {
            health.TakeDamage(damagePerEnemy);
        }
    }
}
