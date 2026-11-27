using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>일정한 주기로 주변 타워를 잠시 멈춘다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Enemy Ability/Tower Disable", fileName = "TowerDisableAbility")]
    public class TowerDisableAbility : EnemyAbility
    {
        [Tooltip("적을 중심으로 한 수평 거리")]
        [SerializeField, Min(0.5f)] private float radius = 6f;
        [Tooltip("타워가 멈추는 시간(초)")]
        [SerializeField, Min(0.1f)] private float duration = 4f;
        [Tooltip("사용 주기(초). 발동하고 한 주기가 지난 뒤에 처음 사용한다")]
        [SerializeField, Min(0.5f)] private float interval = 12f;

        public float Radius => radius;
        public float Duration => duration;
        public float Interval => interval;

        public override EnemyAbilityRuntime CreateRuntime(Enemy owner)
        {
            return new Runtime(this, owner);
        }

        private sealed class Runtime : EnemyAbilityRuntime
        {
            private readonly TowerDisableAbility data;
            private float timer;

            public Runtime(TowerDisableAbility data, Enemy owner) : base(data, owner)
            {
                this.data = data;
            }

            protected override void OnTick(float deltaTime)
            {
                timer += deltaTime;
                if (timer < data.Interval) return;

                timer -= data.Interval;
                float radiusSquared = data.Radius * data.Radius;
                foreach (Tower tower in Tower.Active)
                {
                    Vector3 offset = tower.transform.position - Owner.transform.position;
                    offset.y = 0f;
                    if (offset.sqrMagnitude <= radiusSquared) tower.Disable(data.Duration);
                }
            }
        }
    }
}
