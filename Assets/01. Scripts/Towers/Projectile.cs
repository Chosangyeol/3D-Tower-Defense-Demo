using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 대상 적을 따라가서 피해를 주는 투사체. 대상이 먼저 사라지면 함께 사라진다.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Enemy target;
        private float damage;
        private float speed;
        private float slowRatio;
        private float slowSeconds;
        private float slowedDamageTaken;

        /// <param name="stats">발사하는 순간의 타워 수치</param>
        public void Initialize(Enemy targetEnemy, TowerStats stats)
        {
            target = targetEnemy;
            damage = stats.Damage;
            speed = stats.ProjectileSpeed;
            slowRatio = stats.SlowRatio;
            slowSeconds = stats.SlowSeconds;
            slowedDamageTaken = stats.SlowedDamageTaken;
        }

        private void Update()
        {
            if (target == null || !target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            // 이번 프레임 이동량 안에 들어오면 명중. 빠른 속도에서도 대상을 지나치지 않는다
            float step = speed * Time.deltaTime;
            Vector3 destination = target.AimPosition;
            if (Vector3.Distance(transform.position, destination) <= step)
            {
                target.Health.TakeDamage(damage);
                target.ApplySlow(slowRatio, slowSeconds, slowedDamageTaken);
                Destroy(gameObject);
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, destination, step);
        }
    }
}
