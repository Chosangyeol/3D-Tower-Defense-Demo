using TowerDefense.Effects;

namespace TowerDefense.Towers
{
    /// <summary>이번 판의 효과를 적용한 타워의 최종 공격 수치.</summary>
    public readonly struct TowerStats
    {
        public TowerStats(float damage, float range, float fireInterval, float projectileSpeed,
            int extraTargets, float slowRatio, float slowSeconds)
        {
            Damage = damage;
            Range = range;
            FireInterval = fireInterval;
            ProjectileSpeed = projectileSpeed;
            ExtraTargets = extraTargets;
            SlowRatio = slowRatio;
            SlowSeconds = slowSeconds;
        }

        public float Damage { get; }
        public float Range { get; }
        public float FireInterval { get; }
        public float ProjectileSpeed { get; }
        /// <summary>한 번에 함께 공격하는 추가 대상 수. 효과가 없으면 0.</summary>
        public int ExtraTargets { get; }
        /// <summary>명중한 적의 이동 속도를 줄이는 비율. 효과가 없으면 0.</summary>
        public float SlowRatio { get; }
        /// <summary>감속이 이어지는 시간(초).</summary>
        public float SlowSeconds { get; }

        public static TowerStats From(TowerDefinition tower, RunModifiers modifiers)
        {
            return new TowerStats(
                modifiers.Get(Stat.TowerDamage, tower.Damage, tower),
                modifiers.Get(Stat.TowerRange, tower.Range, tower),
                modifiers.Get(Stat.TowerFireInterval, tower.FireInterval, tower),
                modifiers.Get(Stat.TowerProjectileSpeed, tower.ProjectileSpeed, tower),
                // 아래 수치는 타워 데이터에 기본값이 없고 효과로만 생긴다
                modifiers.GetInt(Stat.TowerExtraTargets, 0f, tower),
                modifiers.Get(Stat.TowerSlowRatio, 0f, tower),
                modifiers.Get(Stat.TowerSlowSeconds, 0f, tower));
        }
    }
}
