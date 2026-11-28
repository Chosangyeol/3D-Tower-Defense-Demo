using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TowerDefense.Data;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Effects
{
    /// <summary>
    /// 수치 하나를 올리거나 내리는 효과.
    /// </summary>
    [CreateAssetMenu(menuName = "Tower Defense/Effect/Stat", fileName = "StatEffect")]
    public class StatEffect : GameEffect
    {
        [SerializeField] private Stat stat;
        [Tooltip("더할 값")]
        [SerializeField] private float flat;
        [Tooltip("비율. 0.2는 +20%, -0.2는 -20%")]
        [SerializeField] private float percent;
        [Tooltip("특정 타워·적에게만 적용할 때 지정. 비우면 효과를 준 쪽의 대상, 그것도 없으면 전체")]
        [SerializeField] private GameData onlyFor;

        public override bool AffectsTowers => IsTowerStat(stat);

        public override void Apply(RunModifiers modifiers, GameData defaultTarget)
        {
            GameData target = onlyFor != null ? onlyFor : Accepts(defaultTarget) ? defaultTarget : null;
            modifiers.Add(stat, flat, percent, target);
        }

        /// <summary>"공격력 +25%"처럼 수치의 이름과 바뀌는 양.</summary>
        public override string Describe()
        {
            string label = GetLabel(stat);
            if (onlyFor != null) label = $"{onlyFor.DisplayName} {label}";

            var parts = new List<string>();
            if (flat != 0f)
            {
                // 비율인 수치에 더하는 값은 퍼센트로 보여준다
                string amount = stat == Stat.TowerSlowedDamageTaken ? Signed(flat * 100f) + "%"
                    : IsRatio(stat) ? Signed(flat * 100f) + "%p"
                    : IsSeconds(stat) ? Signed(flat) + "초"
                    : Signed(flat);
                parts.Add($"{label} {amount}");
            }
            if (percent != 0f) parts.Add($"{label} {Signed(percent * 100f)}%");
            return string.Join(", ", parts);
        }

        private static string Signed(float value)
        {
            return value.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture);
        }

        // 인스펙터에 보이는 "분류/이름"에서 이름만 쓴다
        private static string GetLabel(Stat value)
        {
            FieldInfo field = typeof(Stat).GetField(value.ToString());
            InspectorNameAttribute label = field?.GetCustomAttribute<InspectorNameAttribute>();
            if (label == null) return value.ToString();

            return label.displayName.Substring(label.displayName.IndexOf('/') + 1);
        }

        private static bool IsRatio(Stat value)
        {
            return value == Stat.TowerSlowRatio || value == Stat.SpellSplashRatio;
        }

        private static bool IsSeconds(Stat value)
        {
            switch (value)
            {
                case Stat.TowerFireInterval:
                case Stat.TowerSlowSeconds:
                case Stat.SpellCastInterval:
                case Stat.GatherSeconds:
                case Stat.PrepSeconds:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsTowerStat(Stat value)
        {
            switch (value)
            {
                case Stat.TowerDamage:
                case Stat.TowerRange:
                case Stat.TowerFireInterval:
                case Stat.TowerProjectileSpeed:
                case Stat.TowerCost:
                case Stat.TowerExtraTargets:
                case Stat.TowerSlowRatio:
                case Stat.TowerSlowSeconds:
                case Stat.TowerSplashRadius:
                case Stat.TowerSlowedDamageTaken:
                    return true;
                default:
                    return false;
            }
        }

        // 타워 수치는 타워를, 적 수치와 처치 보상은 적을 대상으로 좁힐 수 있다
        private bool Accepts(GameData target)
        {
            if (IsTowerStat(stat)) return target is TowerDefinition;

            switch (stat)
            {
                case Stat.EnemyCount:
                case Stat.EnemyHealth:
                case Stat.EnemyAttackDamage:
                case Stat.EnemyMoveSpeed:
                case Stat.KillReward:
                    return target is EnemyDefinition;
                default:
                    return false;
            }
        }
    }
}
