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
