using TowerDefense.Combat;
using UnityEngine;

namespace TowerDefense.Upgrades
{
    /// <summary>
    /// 공격 수치 하나를 올리거나 내리는 효과.
    /// </summary>
    [CreateAssetMenu(menuName = "Tower Defense/Upgrade Effect/Stat", fileName = "StatEffect")]
    public class StatEffect : UpgradeEffect
    {
        [SerializeField] private AttackStat stat;
        [Tooltip("더할 값")]
        [SerializeField] private float flat;
        [Tooltip("비율. 0.2는 +20%, -0.2는 -20%")]
        [SerializeField] private float percent;

        public override void Apply(AttackSpec spec)
        {
            spec.Modify(stat, flat, percent);
        }
    }
}
