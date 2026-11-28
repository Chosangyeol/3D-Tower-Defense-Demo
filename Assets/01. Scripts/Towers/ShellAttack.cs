using System.Collections.Generic;
using TowerDefense.Combat;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 쏘는 순간 대상이 있던 위치로 포탄을 떨어뜨린다. 착탄하면 폭발 반경 안의 모든 적이 피해를 입는다.
    /// 대상을 따라가지 않으므로 빠른 적은 벗어날 수 있다.
    /// </summary>
    [CreateAssetMenu(menuName = "Tower Defense/Tower Attack/Shell", fileName = "ShellAttack")]
    public class ShellAttack : TowerAttack
    {
        private const float MinFlightSeconds = 0.1f;

        [SerializeField] private Shell shellPrefab;
        [Tooltip("착탄할 때 범위를 보여주는 표시")]
        [SerializeField] private AreaFlash explosionPrefab;
        [Tooltip("포탄이 가장 높이 올라가는 높이 (보이는 모습에만 영향)")]
        [SerializeField, Min(0f)] private float arcHeight = 2.5f;

        public override void Fire(Tower tower, IReadOnlyList<Enemy> targets, TowerStats stats)
        {
            if (shellPrefab == null) return;

            foreach (Enemy target in targets)
            {
                Vector3 landing = target.transform.position;
                // 비행 시간은 사거리와 같은 수평 거리로 정한다
                Vector3 offset = landing - tower.transform.position;
                offset.y = 0f;
                float seconds = Mathf.Max(offset.magnitude / stats.ProjectileSpeed, MinFlightSeconds);

                Shell shell = Instantiate(shellPrefab, tower.MuzzlePosition, Quaternion.identity);
                shell.Initialize(landing, seconds, arcHeight, stats, explosionPrefab);
            }
        }
    }
}
