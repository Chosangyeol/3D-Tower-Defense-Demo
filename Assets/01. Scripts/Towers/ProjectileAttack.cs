using System.Collections.Generic;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>대상을 따라가는 탄을 대상마다 한 발씩 쏜다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Tower Attack/Projectile", fileName = "ProjectileAttack")]
    public class ProjectileAttack : TowerAttack
    {
        [SerializeField] private Projectile projectilePrefab;

        public override void Fire(Tower tower, IReadOnlyList<Enemy> targets, TowerStats stats)
        {
            if (projectilePrefab == null) return;

            foreach (Enemy target in targets)
            {
                Projectile projectile = Instantiate(projectilePrefab, tower.MuzzlePosition, Quaternion.identity);
                projectile.Initialize(target, stats);
            }
        }
    }
}
