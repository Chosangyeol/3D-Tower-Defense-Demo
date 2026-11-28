using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Structures;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 타워 종류별 데이터. 수치는 플레이 테스트 후 조정한다.
    /// </summary>
    [GameDataCategory("타워", "Towers", 0)]
    [CreateAssetMenu(menuName = "Tower Defense/Tower Definition", fileName = "Tower")]
    public class TowerDefinition : StructureDefinition
    {
        [SerializeField] private Tower prefab;
        [SerializeField, Min(0f)] private float range = 6f;

        [Header("공격")]
        [SerializeField] private TowerAttack attack;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.05f)] private float fireInterval = 0.5f;
        [Tooltip("탄이 날아가는 속도")]
        [SerializeField, Min(1f)] private float projectileSpeed = 25f;

        [Header("특수 (쓰지 않으면 0)")]
        [Tooltip("착탄한 곳에서 피해가 닿는 반경")]
        [SerializeField, Min(0f)] private float splashRadius;
        [Tooltip("맞은 적의 이동 속도를 줄이는 비율. 0.5는 절반")]
        [SerializeField, Range(0f, 0.9f)] private float slowRatio;
        [Tooltip("감속이 이어지는 시간(초)")]
        [SerializeField, Min(0f)] private float slowSeconds;

        public override Structure Prefab => prefab;
        public float Range => range;
        public TowerAttack Attack => attack;
        public float Damage => damage;
        public float FireInterval => fireInterval;
        public float ProjectileSpeed => projectileSpeed;
        public float SplashRadius => splashRadius;
        public float SlowRatio => slowRatio;
        public float SlowSeconds => slowSeconds;

        public override List<ResourceAmount> GetCost(RunModifiers modifiers)
        {
            return modifiers.Scale(Cost, Stat.TowerCost, 1f, this);
        }

        public override float GetRange(RunModifiers modifiers)
        {
            return modifiers.Get(Stat.TowerRange, range, this);
        }
    }
}
