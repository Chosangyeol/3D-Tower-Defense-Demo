using TowerDefense.Effects;

namespace TowerDefense.Towers
{
    /// <summary>이번 판의 효과를 적용한 타워의 최종 공격 수치.</summary>
    public readonly struct TowerStats
    {
        public TowerStats(float damage, float range, float fireInterval, float projectileSpeed)
        {
            Damage = damage;
            Range = range;
            FireInterval = fireInterval;
            ProjectileSpeed = projectileSpeed;
        }

        public float Damage { get; }
        public float Range { get; }
        public float FireInterval { get; }
        public float ProjectileSpeed { get; }

        public static TowerStats From(TowerDefinition tower, RunModifiers modifiers)
        {
            return new TowerStats(
                modifiers.Get(Stat.TowerDamage, tower.Damage, tower),
                modifiers.Get(Stat.TowerRange, tower.Range, tower),
                modifiers.Get(Stat.TowerFireInterval, tower.FireInterval, tower),
                modifiers.Get(Stat.TowerProjectileSpeed, tower.ProjectileSpeed, tower));
        }
    }
}
