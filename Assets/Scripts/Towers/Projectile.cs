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

        public void Initialize(Enemy targetEnemy, float hitDamage, float moveSpeed)
        {
            target = targetEnemy;
            damage = hitDamage;
            speed = moveSpeed;
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
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, destination, step);
        }
    }
}
