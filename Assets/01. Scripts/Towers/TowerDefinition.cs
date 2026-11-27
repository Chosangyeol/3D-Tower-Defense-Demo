using TowerDefense.Combat;
using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 타워 종류별 데이터. 수치는 플레이 테스트 후 조정한다.
    /// </summary>
    [GameDataCategory("타워", "Towers", 0)]
    [CreateAssetMenu(menuName = "Tower Defense/Tower Definition", fileName = "Tower")]
    public class TowerDefinition : GameData
    {
        [SerializeField] private Tower prefab;
        [SerializeField, Min(0f)] private float range = 6f;

        [Header("공격")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.05f)] private float fireInterval = 0.5f;
        [SerializeField, Min(1f)] private float projectileSpeed = 25f;

        public Tower Prefab => prefab;
        public float Range => range;
        public Projectile ProjectilePrefab => projectilePrefab;
        public float Damage => damage;
        public float FireInterval => fireInterval;
        public float ProjectileSpeed => projectileSpeed;

        /// <summary>업그레이드를 적용하기 전의 공격 정보를 만든다.</summary>
        public AttackSpec CreateSpec()
        {
            return new AttackSpec(damage, range, fireInterval, projectileSpeed);
        }
    }
}
