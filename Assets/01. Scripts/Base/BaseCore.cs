using TowerDefense.Combat;
using TowerDefense.Effects;
using TowerDefense.Enemies;
using TowerDefense.Game;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Base
{
    /// <summary>
    /// 지켜야 할 기지. 도착한 적들이 같은 오브젝트의 Health를 직접 공격한다.
    /// 최대 체력과 위치(경로의 끝)는 맵 데이터를 따른다. 회복 효과가 있으면 웨이브를 막을 때마다 체력을 되찾는다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class BaseCore : MonoBehaviour
    {
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunModifiers modifiers;
        [SerializeField] private EnemyPath path;
        [SerializeField] private WaveSpawner waveSpawner;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Initialize(modifiers.GetInt(Stat.BaseHealth, setup.Map.BaseHealth));
        }

        private void OnEnable()
        {
            waveSpawner.WaveCleared += OnWaveCleared;
        }

        private void OnDisable()
        {
            waveSpawner.WaveCleared -= OnWaveCleared;
        }

        private void OnWaveCleared(int wave)
        {
            health.Heal(modifiers.GetInt(Stat.BaseHealPerWave, 0f));
        }

        private void Start()
        {
            if (path.PointCount == 0) return;

            // 높이는 씬에 놓은 그대로 둔다
            Vector3 end = path.GetPoint(path.PointCount - 1);
            transform.position = new Vector3(end.x, transform.position.y, end.z);
        }
    }
}
