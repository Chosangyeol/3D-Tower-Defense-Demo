using TowerDefense.Effects;

namespace TowerDefense.Magic
{
    /// <summary>이번 판의 효과를 적용한 마법의 최종 수치.</summary>
    public readonly struct SpellStats
    {
        public SpellStats(float damage, float manaCost, float castInterval,
            int extraPierce, float splashRadius, float splashRatio)
        {
            Damage = damage;
            ManaCost = manaCost;
            CastInterval = castInterval;
            ExtraPierce = extraPierce;
            SplashRadius = splashRadius;
            SplashRatio = splashRatio;
        }

        public float Damage { get; }
        public float ManaCost { get; }
        public float CastInterval { get; }
        /// <summary>처음 맞힌 적 뒤로 더 맞힐 수 있는 적의 수. 효과가 없으면 0.</summary>
        public int ExtraPierce { get; }
        /// <summary>맞은 곳에서 폭발이 닿는 거리. 효과가 없으면 0.</summary>
        public float SplashRadius { get; }
        /// <summary>폭발 피해가 마법 피해에서 차지하는 비율.</summary>
        public float SplashRatio { get; }

        public static SpellStats From(SpellDefinition spell, RunModifiers modifiers)
        {
            return new SpellStats(
                modifiers.Get(Stat.SpellDamage, spell.Damage, spell),
                modifiers.Get(Stat.SpellManaCost, spell.ManaCost, spell),
                modifiers.Get(Stat.SpellCastInterval, spell.CastInterval, spell),
                // 아래 수치는 마법 데이터에 기본값이 없고 효과로만 생긴다
                modifiers.GetInt(Stat.SpellPierce, 0f, spell),
                modifiers.Get(Stat.SpellSplashRadius, 0f, spell),
                modifiers.Get(Stat.SpellSplashRatio, 0f, spell));
        }
    }
}
