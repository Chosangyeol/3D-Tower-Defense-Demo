using TowerDefense.Combat;
using TowerDefense.Enemies;
using TowerDefense.Game;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Base
{
    /// <summary>
    /// 지켜야 할 기지. 적이 도달할 때마다 적 데이터의 피해만큼 체력이 줄어든다.
    /// 체력은 같은 오브젝트의 Health가 관리하고, 최대 체력과 위치(경로의 끝)는 스테이지 데이터를 따른다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class BaseCore : MonoBehaviour
    {
        [SerializeField] private StageController stageController;
        [SerializeField] private EnemyPath path;
        [SerializeField] private WaveSpawner waveSpawner;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Initialize(stageController.Stage.BaseHealth);
        }

        private void Start()
        {
            if (path.PointCount == 0) return;

            // 높이는 씬에 놓은 그대로 둔다
            Vector3 end = path.GetPoint(path.PointCount - 1);
            transform.position = new Vector3(end.x, transform.position.y, end.z);
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
            health.TakeDamage(enemy.Definition.BaseDamage);
        }
    }
}
