using System;
using System.Collections.Generic;
using TowerDefense.Combat;
using UnityEngine;

namespace TowerDefense.Magic
{
    /// <summary>
    /// 플레이어의 마법 투사체. 조준한 방향으로 직선 비행하며, 처음 부딪힌 적에게 피해를 준다.
    /// 관통 효과가 있으면 그 수만큼 적을 더 뚫고 지나가며 같은 적은 한 번만 맞힌다. 지면과 타워는 뚫지 못한다.
    /// 폭발 효과가 있으면 적이나 지면·타워에 맞을 때마다 그 주변의 적에게 추가 피해를 준다.
    /// 직접 맞은 적은 그 폭발의 피해를 받지 않는다.
    /// 판정은 조준선을 따라가고, 보이는 모형만 손 위치에서 출발해 조준선으로 모인다.
    /// </summary>
    public class SpellProjectile : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private Vector3 visualStartOffset = new Vector3(0.35f, -0.3f, 1.1f);
        [SerializeField, Min(0.01f)] private float visualBlendTime = 0.2f;
        [Tooltip("폭발 범위를 보여주는 표시")]
        [SerializeField] private SplashFlash splashFlash;

        private static readonly RaycastHit[] hits = new RaycastHit[32];
        private static readonly IComparer<RaycastHit> ByDistance =
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

        private readonly HashSet<IDamageable> struck = new HashSet<IDamageable>();

        private Transform caster;
        private SpellDefinition spell;
        private SpellStats stats;
        private int pierceLeft;
        private float travelled;
        private float age;

        /// <param name="castStats">시전하는 순간의 마법 수치</param>
        public void Initialize(Transform casterRoot, SpellDefinition definition, SpellStats castStats)
        {
            caster = casterRoot;
            spell = definition;
            stats = castStats;
            pierceLeft = Math.Max(castStats.ExtraPierce, 0);
            UpdateVisual();
        }

        private void Update()
        {
            if (spell == null) return;

            age += Time.deltaTime;
            float step = Mathf.Min(spell.ProjectileSpeed * Time.deltaTime, spell.MaxDistance - travelled);

            // 이번 프레임에 지나는 구간 전체를 검사해서, 빠른 속도에서도 통과하지 않는다
            if (HitAlong(step))
            {
                Destroy(gameObject);
                return;
            }

            transform.position += transform.forward * step;
            travelled += step;
            if (travelled >= spell.MaxDistance)
            {
                Destroy(gameObject);
                return;
            }
            UpdateVisual();
        }

        // 구간에서 부딪히는 것을 가까운 순서로 처리한다. 투사체가 여기서 사라져야 하면 true
        private bool HitAlong(float distance)
        {
            int count = Physics.SphereCastNonAlloc(transform.position, spell.HitRadius, transform.forward, hits, distance,
                Physics.AllLayers, QueryTriggerInteraction.Collide);
            Array.Sort(hits, 0, count, ByDistance);

            for (int i = 0; i < count; i++)
            {
                Collider candidate = hits[i].collider;
                if (caster != null && candidate.transform.IsChildOf(caster)) continue;

                // 살아 있는 적과 단단한 물체(지면, 타워)에만 부딪힌다
                IDamageable target = candidate.GetComponentInParent<IDamageable>();
                if (target != null && target.Team == Team.Invaders)
                {
                    if (!target.IsAlive || !struck.Add(target)) continue;

                    target.TakeDamage(stats.Damage);
                    Explode(PointAt(hits[i].distance), target);
                    if (pierceLeft <= 0) return true;

                    pierceLeft--;
                }
                else if (!candidate.isTrigger)
                {
                    Explode(PointAt(hits[i].distance), null);
                    return true;
                }
            }
            return false;
        }

        // 부딪힌 순간의 투사체 중심
        private Vector3 PointAt(float distance)
        {
            return transform.position + transform.forward * distance;
        }

        /// <param name="direct">직접 맞아서 폭발 피해에서 빼는 대상. 지면에 맞았으면 null</param>
        private void Explode(Vector3 center, IDamageable direct)
        {
            float damage = stats.Damage * stats.SplashRatio;
            if (stats.SplashRadius <= 0f || damage <= 0f) return;

            // 기지 앞에 많은 적이 겹쳐 설 수 있어 개수를 제한하지 않는다
            Collider[] reached = Physics.OverlapSphere(center, stats.SplashRadius, Physics.AllLayers,
                QueryTriggerInteraction.Collide);
            var damaged = new HashSet<IDamageable>();
            foreach (Collider candidate in reached)
            {
                IDamageable target = candidate.GetComponentInParent<IDamageable>();
                if (target == null || target == direct || target.Team != Team.Invaders) continue;
                if (target.IsAlive && damaged.Add(target)) target.TakeDamage(damage);
            }

            if (splashFlash != null) Instantiate(splashFlash, center, Quaternion.identity).Show(stats.SplashRadius);
        }

        private void UpdateVisual()
        {
            if (visual == null) return;

            float blend = Mathf.Clamp01(age / visualBlendTime);
            visual.localPosition = Vector3.Lerp(visualStartOffset, Vector3.zero, blend);
        }
    }
}
