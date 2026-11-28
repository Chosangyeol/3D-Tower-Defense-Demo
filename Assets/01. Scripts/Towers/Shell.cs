using System.Collections.Generic;
using TowerDefense.Combat;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 정해진 지점으로 곡선을 그리며 날아가서 폭발하는 포탄. 날아가는 동안에는 아무것에도 부딪히지 않는다.
    /// </summary>
    public class Shell : MonoBehaviour
    {
        private Vector3 start;
        private Vector3 landing;
        private float flightSeconds;
        private float arcHeight;
        private float age;
        private TowerStats stats;
        private AreaFlash explosion;

        /// <param name="landingPoint">떨어질 지면 위의 위치</param>
        /// <param name="seconds">도착까지 걸리는 시간</param>
        /// <param name="towerStats">발사하는 순간의 타워 수치</param>
        public void Initialize(Vector3 landingPoint, float seconds, float height, TowerStats towerStats,
            AreaFlash explosionPrefab)
        {
            start = transform.position;
            landing = landingPoint;
            flightSeconds = seconds;
            arcHeight = height;
            stats = towerStats;
            explosion = explosionPrefab;
        }

        private void Update()
        {
            if (flightSeconds <= 0f) return;

            age += Time.deltaTime;
            float progress = Mathf.Clamp01(age / flightSeconds);
            float lift = 4f * arcHeight * progress * (1f - progress);
            transform.position = Vector3.Lerp(start, landing, progress) + Vector3.up * lift;
            if (progress >= 1f) Explode();
        }

        private void Explode()
        {
            // 처치되면서 목록이 바뀌어도 되도록 복사해서 돈다
            foreach (Enemy enemy in new List<Enemy>(Enemy.Active))
            {
                if (!enemy.IsAlive) continue;

                // 적의 몸이 폭발 범위에 걸치면 맞는다. 높이는 보지 않는다
                Vector3 offset = enemy.transform.position - landing;
                offset.y = 0f;
                if (offset.magnitude > stats.SplashRadius + enemy.BodyRadius) continue;

                enemy.Health.TakeDamage(stats.Damage);
                enemy.ApplySlow(stats.SlowRatio, stats.SlowSeconds, stats.SlowedDamageTaken);
            }

            if (explosion != null) Instantiate(explosion, landing, Quaternion.identity).Show(stats.SplashRadius);
            Destroy(gameObject);
        }
    }
}
