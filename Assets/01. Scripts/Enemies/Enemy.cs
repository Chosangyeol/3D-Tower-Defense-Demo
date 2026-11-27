using System;
using System.Collections.Generic;
using TowerDefense.Combat;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>적이 사라진 이유.</summary>
    public enum EnemyOutcome
    {
        Killed,      // 처치됨
        ReachedGoal, // 기지에 도달
        Removed      // 그 밖의 제거 (씬 종료 등)
    }

    /// <summary>
    /// 경로를 따라 기지까지 이동하는 적. 체력은 같은 오브젝트의 Health가 관리하고, 0이 되면 사라진다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        private static readonly List<Enemy> active = new List<Enemy>();

        [SerializeField] private Transform aimPoint;

        private EnemyPath path;
        private int targetIndex;
        private bool finished;

        /// <summary>씬에 있는 적 목록. 타겟을 고를 때 사용한다.</summary>
        public static IReadOnlyList<Enemy> Active => active;

        public EnemyDefinition Definition { get; private set; }
        public Health Health { get; private set; }
        /// <summary>경로를 따라 이동한 거리. 클수록 기지에 가깝다.</summary>
        public float DistanceTravelled { get; private set; }
        public bool IsAlive => path != null && !finished && Health.IsAlive;
        /// <summary>투사체가 향하는 위치.</summary>
        public Vector3 AimPosition => aimPoint != null ? aimPoint.position : transform.position;

        /// <summary>적이 사라질 때 한 번 호출된다.</summary>
        public event Action<Enemy, EnemyOutcome> Finished;

        public void Initialize(EnemyDefinition definition, EnemyPath enemyPath)
        {
            Definition = definition;
            Health.Initialize(definition.MaxHealth);
            path = enemyPath;
            targetIndex = 1;
            transform.position = path.GetPoint(0);
            FaceTarget();
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

            float remaining = Definition.MoveSpeed * Time.deltaTime;
            while (remaining > 0f)
            {
                Vector3 target = path.GetPoint(targetIndex);
                float distance = Vector3.Distance(transform.position, target);
                if (distance > remaining)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, remaining);
                    DistanceTravelled += remaining;
                    return;
                }

                // 꺾이는 지점에서 남은 이동량을 다음 구간에 이어서 쓴다
                transform.position = target;
                DistanceTravelled += distance;
                remaining -= distance;
                targetIndex++;
                if (targetIndex >= path.PointCount)
                {
                    Finish(EnemyOutcome.ReachedGoal);
                    Destroy(gameObject);
                    return;
                }
                FaceTarget();
            }
        }

        private void OnDestroy()
        {
            Health.Died -= OnDied;
            Finish(EnemyOutcome.Removed);
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
