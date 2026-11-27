using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 설치된 타워. 사거리 안에서 가장 앞선 적을 골라 포탑을 돌리고 투사체를 쏜다.
    /// 배치·전투 모드와 관계없이 동작한다.
    /// </summary>
    public class Tower : MonoBehaviour
    {
        [SerializeField] private Transform turret;
        [SerializeField] private Transform muzzle;

        private float cooldown;

        public TowerDefinition Definition { get; private set; }
        public Vector2Int Cell { get; private set; }
        public Enemy Target { get; private set; }

        public void Initialize(TowerDefinition definition, Vector2Int cell)
        {
            Definition = definition;
            Cell = cell;
        }

        private void Update()
        {
            if (Definition == null) return;

            cooldown = Mathf.Max(cooldown - Time.deltaTime, 0f);
            Target = FindTarget();
            if (Target == null) return;

            Aim(Target.AimPosition);
            if (cooldown > 0f) return;

            Fire(Target);
            cooldown = Definition.FireInterval;
        }

        private Enemy FindTarget()
        {
            Enemy best = null;
            float rangeSquared = Definition.Range * Definition.Range;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (!enemy.IsAlive) continue;

                // 사거리 원과 같게 수평 거리만 본다
                Vector3 offset = enemy.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > rangeSquared) continue;

                if (best == null || enemy.DistanceTravelled > best.DistanceTravelled) best = enemy;
            }
            return best;
        }

        private void Aim(Vector3 position)
        {
            if (turret == null) return;

            Vector3 direction = position - turret.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f) turret.rotation = Quaternion.LookRotation(direction);
        }

        private void Fire(Enemy target)
        {
            if (Definition.ProjectilePrefab == null) return;

            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            Projectile projectile = Instantiate(Definition.ProjectilePrefab, origin, Quaternion.identity);
            projectile.Initialize(target, Definition.Damage, Definition.ProjectileSpeed);
        }
    }
}
