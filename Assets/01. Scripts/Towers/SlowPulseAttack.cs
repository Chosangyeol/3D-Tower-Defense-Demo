using System.Collections.Generic;
using TowerDefense.Combat;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>사거리 안의 모든 적을 잠시 느리게 만드는 파동. 피해는 주지 않는다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Tower Attack/Slow Pulse", fileName = "SlowPulseAttack")]
    public class SlowPulseAttack : TowerAttack
    {
        [Tooltip("파동이 퍼진 범위를 보여주는 표시")]
        [SerializeField] private AreaFlash pulsePrefab;
        [SerializeField, Min(0f)] private float pulseHeight = 0.15f;

        public override bool AimsTurret => false;

        public override int GetTargetCount(TowerStats stats)
        {
            return int.MaxValue;
        }

        public override void Fire(Tower tower, IReadOnlyList<Enemy> targets, TowerStats stats)
        {
            foreach (Enemy target in targets)
            {
                target.ApplySlow(stats.SlowRatio, stats.SlowSeconds);
            }

            if (pulsePrefab == null) return;

            Vector3 center = tower.transform.position + Vector3.up * pulseHeight;
            Instantiate(pulsePrefab, center, Quaternion.identity).Show(stats.Range);
        }
    }
}
