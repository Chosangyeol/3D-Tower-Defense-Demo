using System;
using System.Collections.Generic;
using TowerDefense.Combat;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>적이 사라진 이유.</summary>
    public enum EnemyOutcome
    {
        Killed, // 처치됨
        Removed // 그 밖의 제거 (씬 종료 등)
    }

    /// <summary>
    /// 경로를 따라 기지 앞까지 이동한 뒤, 기지가 무너질 때까지 공격하는 적.
    /// 체력은 같은 오브젝트의 Health가 관리하고, 0이 되면 사라진다. 능력은 적 데이터를 따른다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        private static readonly List<Enemy> active = new List<Enemy>();

        [SerializeField] private Transform aimPoint;
        [Tooltip("몸의 반지름. 기지와 겹치지 않고 멈추는 위치를 정한다")]
        [SerializeField, Min(0f)] private float bodyRadius = 0.45f;

        private readonly List<EnemyAbilityRuntime> abilities = new List<EnemyAbilityRuntime>();

        private EnemyPath path;
        private IDamageable goal;
        private int targetIndex;
        private float stopDistance;
        private float attackCooldown;
        private float speedBonus;
        private bool finished;

        /// <summary>씬에 있는 적 목록. 타겟을 고를 때 사용한다.</summary>
        public static IReadOnlyList<Enemy> Active => active;

        public EnemyDefinition Definition { get; private set; }
        public Health Health { get; private set; }
        /// <summary>경로를 따라 이동한 거리. 클수록 기지에 가깝다.</summary>
        public float DistanceTravelled { get; private set; }
        /// <summary>기지 앞에 도착해서 공격 중인지 여부.</summary>
        public bool IsAttacking { get; private set; }
        public bool IsAlive => path != null && !finished && Health.IsAlive;
        /// <summary>능력의 속도 변화를 반영한 이동 속도.</summary>
        public float MoveSpeed => Definition.MoveSpeed * Mathf.Max(1f + speedBonus, 0f);
        /// <summary>투사체가 향하는 위치.</summary>
        public Vector3 AimPosition => aimPoint != null ? aimPoint.position : transform.position;

        /// <summary>적이 사라질 때 한 번 호출된다.</summary>
        public event Action<Enemy, EnemyOutcome> Finished;
        /// <summary>능력이 부하를 부를 때 호출된다. (부른 적, 부하의 데이터, 수)</summary>
        public event Action<Enemy, EnemyDefinition, int> SummonRequested;

        /// <param name="goalTarget">도착해서 공격할 대상 (기지)</param>
        /// <param name="startDistance">경로의 시작점에서 이만큼 나아간 곳에서 출발한다</param>
        public void Initialize(EnemyDefinition definition, EnemyPath enemyPath, IDamageable goalTarget,
            float startDistance = 0f)
        {
            Definition = definition;
            Health.Initialize(definition.MaxHealth);
            path = enemyPath;
            goal = goalTarget;
            stopDistance = Mathf.Max(path.Length - path.GoalRadius - bodyRadius, 0f);
            targetIndex = 1;
            transform.position = path.GetPoint(0);
            FaceTarget();
            Move(startDistance);

            abilities.Clear();
            foreach (EnemyAbility ability in definition.Abilities)
            {
                if (ability != null) abilities.Add(ability.CreateRuntime(this));
            }
        }

        /// <summary>속도 비율을 더한다. 0.3은 +30%.</summary>
        public void AddSpeedBonus(float percent)
        {
            speedBonus += percent;
        }

        public void RequestSummon(EnemyDefinition summoned, int count)
        {
            if (IsAlive && summoned != null && count > 0) SummonRequested?.Invoke(this, summoned, count);
        }

        private void Awake()
        {
            Health = GetComponent<Health>();
            Health.Died += OnDied;
        }

        private void OnEnable()
        {
            active.Add(this);
        }

        private void OnDisable()
        {
            active.Remove(this);
        }

        private void Update()
        {
            if (!IsAlive) return;

            UpdateAbilities(Time.deltaTime);
            if (IsAttacking) Attack(Time.deltaTime);
            else Move(MoveSpeed * Time.deltaTime);
        }

        private void OnDestroy()
        {
            Health.Died -= OnDied;
            Finish(EnemyOutcome.Removed);
        }

        private void UpdateAbilities(float deltaTime)
        {
            float healthRatio = Health.Current / Health.Max;
            foreach (EnemyAbilityRuntime ability in abilities)
            {
                ability.Update(healthRatio, deltaTime);
            }
        }

        private void Move(float amount)
        {
            float remaining = Mathf.Min(amount, stopDistance - DistanceTravelled);
            while (remaining > 0f && targetIndex < path.PointCount)
            {
                Vector3 target = path.GetPoint(targetIndex);
                float distance = Vector3.Distance(transform.position, target);
                if (distance > remaining)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, remaining);
                    DistanceTravelled += remaining;
                    break;
                }

                // 꺾이는 지점에서 남은 이동량을 다음 구간에 이어서 쓴다
                transform.position = target;
                DistanceTravelled += distance;
                remaining -= distance;
                targetIndex++;
                if (targetIndex < path.PointCount) FaceTarget();
            }

            if (!IsAttacking && DistanceTravelled >= stopDistance - 0.001f)
            {
                IsAttacking = true;
                // 도착하고 한 번의 공격 간격이 지난 뒤에 첫 공격을 한다
                attackCooldown = Definition.AttackInterval;
            }
        }

        private void Attack(float deltaTime)
        {
            if (goal == null || !goal.IsAlive) return;

            attackCooldown -= deltaTime;
            while (attackCooldown <= 0f && goal.IsAlive)
            {
                goal.TakeDamage(Definition.AttackDamage);
                attackCooldown += Definition.AttackInterval;
            }
        }

        private void OnDied(Health health)
        {
            Finish(EnemyOutcome.Killed);
            Destroy(gameObject);
        }

        private void FaceTarget()
        {
            Vector3 direction = path.GetPoint(targetIndex) - transform.position;
            if (direction.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        private void Finish(EnemyOutcome outcome)
        {
            if (finished) return;

            finished = true;
            Finished?.Invoke(this, outcome);
        }
    }
}
