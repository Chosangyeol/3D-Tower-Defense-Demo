using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Magic
{
    /// <summary>
    /// 마법 종류별 데이터. 수치는 플레이 테스트 후 조정한다.
    /// </summary>
    [GameDataCategory("마법", "Spells", 20)]
    [CreateAssetMenu(menuName = "Tower Defense/Spell Definition", fileName = "Spell")]
    public class SpellDefinition : GameData
    {
        [SerializeField] private SpellProjectile projectilePrefab;
        [SerializeField, Min(0f)] private float manaCost = 10f;
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.05f)] private float castInterval = 0.35f;

        [Header("투사체")]
        [SerializeField, Min(1f)] private float projectileSpeed = 35f;
        [SerializeField, Min(0.01f)] private float hitRadius = 0.2f;
        [SerializeField, Min(1f)] private float maxDistance = 60f;

        public SpellProjectile ProjectilePrefab => projectilePrefab;
        public float ManaCost => manaCost;
        public float Damage => damage;
        public float CastInterval => castInterval;
        public float ProjectileSpeed => projectileSpeed;
        public float HitRadius => hitRadius;
        public float MaxDistance => maxDistance;
    }
}
