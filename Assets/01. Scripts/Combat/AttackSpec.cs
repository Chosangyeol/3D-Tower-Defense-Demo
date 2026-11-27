using UnityEngine;

namespace TowerDefense.Combat
{
    /// <summary>업그레이드가 바꿀 수 있는 공격 수치.</summary>
    public enum AttackStat
    {
        Damage,         // 공격력
        Range,          // 사거리
        FireInterval,   // 발사 간격(초). 줄일수록 빨라진다
        ProjectileSpeed // 투사체 속도
    }

    /// <summary>
    /// 기본 수치에 업그레이드를 적용한 최종 공격 정보. 실행 중에만 존재하며 데이터 에셋은 바꾸지 않는다.
    /// 최종값 = (기본값 + 더하기 합) × (1 + 비율 합). 적용 순서에 따라 결과가 달라지지 않는다.
    /// 발사 갈래 수 같은 특수 기능은 여기에 항목을 추가한다.
    /// </summary>
    public sealed class AttackSpec
    {
        private static readonly float[] Minimums = { 0f, 0f, 0.05f, 1f };

        private readonly float[] baseValues;
        private readonly float[] flat;
        private readonly float[] percent;

        public AttackSpec(float damage, float range, float fireInterval, float projectileSpeed)
        {
            baseValues = new[] { damage, range, fireInterval, projectileSpeed };
            flat = new float[baseValues.Length];
            percent = new float[baseValues.Length];
        }

        public float Damage => Get(AttackStat.Damage);
        public float Range => Get(AttackStat.Range);
        public float FireInterval => Get(AttackStat.FireInterval);
        public float ProjectileSpeed => Get(AttackStat.ProjectileSpeed);

        public float Get(AttackStat stat)
        {
            int i = (int)stat;
            return Mathf.Max((baseValues[i] + flat[i]) * (1f + percent[i]), Minimums[i]);
        }

        /// <param name="flatAmount">더할 값</param>
        /// <param name="percentAmount">비율. 0.2는 +20%, -0.2는 -20%</param>
        public void Modify(AttackStat stat, float flatAmount, float percentAmount)
        {
            flat[(int)stat] += flatAmount;
            percent[(int)stat] += percentAmount;
        }
    }
}
