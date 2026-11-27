using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Player
{
    /// <summary>
    /// 플레이어의 마법 투사체. 조준한 방향으로 직선 비행하며, 처음 부딪힌 적 하나에게 피해를 준다.
    /// 판정은 조준선을 따라가고, 보이는 모형만 손 위치에서 출발해 조준선으로 모인다.
    /// </summary>
    public class SpellProjectile : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField, Min(0.01f)] private float radius = 0.2f;
        [SerializeField, Min(1f)] private float maxDistance = 60f;
        [SerializeField] private Vector3 visualStartOffset = new Vector3(0.35f, -0.3f, 1.1f);
        [SerializeField, Min(0.01f)] private float visualBlendTime = 0.2f;

        private static readonly RaycastHit[] hits = new RaycastHit[16];

        private Transform caster;
        private float damage;
        private float speed;
        private float travelled;
        private float age;

        public void Initialize(Transform casterRoot, float hitDamage, float moveSpeed)
        {
            caster = casterRoot;
            damage = hitDamage;
            speed = moveSpeed;
            UpdateVisual();
        }

        private void Update()
        {
            if (speed <= 0f) return;

            age += Time.deltaTime;
            float step = Mathf.Min(speed * Time.deltaTime, maxDistance - travelled);

            // 이번 프레임에 지나는 구간 전체를 검사해서, 빠른 속도에서도 통과하지 않는다
            if (TryFindHit(step, out Collider hit))
            {
                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null) enemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            transform.position += transform.forward * step;
            travelled += step;
            if (travelled >= maxDistance)
            {
                Destroy(gameObject);
                return;
            }
            UpdateVisual();
        }

        private bool TryFindHit(float distance, out Collider nearest)
        {
            nearest = null;
            float nearestDistance = float.MaxValue;
            int count = Physics.SphereCastNonAlloc(transform.position, radius, transform.forward, hits, distance,
                Physics.AllLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider candidate = hits[i].collider;
                if (caster != null && candidate.transform.IsChildOf(caster)) continue;

                // 살아 있는 적과 단단한 물체(지면, 타워)에만 부딪힌다
                Enemy enemy = candidate.GetComponentInParent<Enemy>();
                bool blocks = enemy != null ? enemy.IsAlive : !candidate.isTrigger;
                if (!blocks || hits[i].distance >= nearestDistance) continue;

                nearest = candidate;
                nearestDistance = hits[i].distance;
            }
            return nearest != null;
        }

        private void UpdateVisual()
        {
            if (visual == null) return;

            float blend = Mathf.Clamp01(age / visualBlendTime);
            visual.localPosition = Vector3.Lerp(visualStartOffset, Vector3.zero, blend);
        }
    }
}
