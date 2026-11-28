using System.Collections.Generic;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 설치된 타워. 사거리 안에서 앞선 적을 골라 타워 데이터의 공격 방식으로 공격한다.
    /// 배치·전투 모드와 관계없이 동작한다. 적의 능력으로 잠시 멈출 수 있다.
    /// </summary>
    public class Tower : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color DisabledColor = new Color(0.25f, 0.3f, 0.6f);
        private static readonly List<Tower> active = new List<Tower>();

        [SerializeField] private Transform turret;
        [SerializeField] private Transform muzzle;

        private readonly List<Enemy> targets = new List<Enemy>();

        private float cooldown;
        private RunModifiers modifiers;
        private float disabledRemaining;

        /// <summary>씬에 설치된 타워 목록. 배치 미리보기는 포함하지 않는다.</summary>
        public static IReadOnlyList<Tower> Active => active;

        public bool IsDisabled => disabledRemaining > 0f;

        public TowerDefinition Definition { get; private set; }
        public Vector2Int Cell { get; private set; }
        public Enemy Target { get; private set; }

        /// <summary>설치할 때 낸 비용. 환급은 이 값을 기준으로 한다.</summary>
        public IReadOnlyList<ResourceAmount> PaidCost { get; private set; }
        /// <summary>이번 판의 효과를 적용한 지금의 공격 수치.</summary>
        public TowerStats Stats => TowerStats.From(Definition, modifiers);
        /// <summary>탄이 나가는 위치.</summary>
        public Vector3 MuzzlePosition => muzzle != null ? muzzle.position : transform.position;

        public void Initialize(TowerDefinition definition, Vector2Int cell, RunModifiers runModifiers,
            IReadOnlyList<ResourceAmount> paidCost)
        {
            Definition = definition;
            Cell = cell;
            modifiers = runModifiers;
            PaidCost = paidCost;
        }

        /// <summary>정해진 시간 동안 공격을 멈춘다. 이미 멈춰 있으면 더 긴 쪽을 따른다.</summary>
        public void Disable(float seconds)
        {
            if (seconds <= 0f) return;

            if (!IsDisabled) Tint(true);
            disabledRemaining = Mathf.Max(disabledRemaining, seconds);
            Target = null;
        }

        private void OnEnable()
        {
            active.Add(this);
        }

        private void OnDisable()
        {
            active.Remove(this);
        }

        // 멈춘 타워는 파랗게 어둡게 보여준다. 머티리얼 에셋은 바꾸지 않는다
        private void Tint(bool disabled)
        {
            MaterialPropertyBlock block = null;
            if (disabled)
            {
                block = new MaterialPropertyBlock();
                block.SetColor(BaseColorId, DisabledColor);
            }
            foreach (Renderer part in GetComponentsInChildren<Renderer>())
            {
                part.SetPropertyBlock(block);
            }
        }

        private void Update()
        {
            if (Definition == null || Time.deltaTime <= 0f) return;

            if (IsDisabled)
            {
                disabledRemaining -= Time.deltaTime;
                if (IsDisabled) return;

                disabledRemaining = 0f;
                Tint(false);
            }

            cooldown = Mathf.Max(cooldown - Time.deltaTime, 0f);
            TowerAttack attack = Definition.Attack;
            if (attack == null) return;

            TowerStats stats = Stats;
            FindTargets(stats.Range, attack.GetTargetCount(stats));
            Target = targets.Count > 0 ? targets[0] : null;
            if (Target == null) return;

            if (attack.AimsTurret) Aim(Target.AimPosition);
            if (cooldown > 0f) return;

            attack.Fire(this, targets, stats);
            cooldown = stats.FireInterval;
        }

        // 사거리 안의 적을 앞선 순서로 최대 수만큼 고른다
        private void FindTargets(float range, int maxCount)
        {
            targets.Clear();
            float rangeSquared = range * range;

            foreach (Enemy enemy in Enemy.Active)
            {
                if (!enemy.IsAlive) continue;

                // 사거리 원과 같게 수평 거리만 본다
                Vector3 offset = enemy.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > rangeSquared) continue;

                int index = 0;
                while (index < targets.Count && targets[index].DistanceTravelled >= enemy.DistanceTravelled) index++;
                if (index >= maxCount) continue;

                targets.Insert(index, enemy);
                if (targets.Count > maxCount) targets.RemoveAt(maxCount);
            }
        }

        private void Aim(Vector3 position)
        {
            if (turret == null) return;

            Vector3 direction = position - turret.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f) turret.rotation = Quaternion.LookRotation(direction);
        }
    }
}
