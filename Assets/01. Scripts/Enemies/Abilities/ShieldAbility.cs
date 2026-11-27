using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>발동하면 정해진 시간 동안 받는 피해가 줄어든다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Enemy Ability/Shield", fileName = "ShieldAbility")]
    public class ShieldAbility : EnemyAbility
    {
        [Tooltip("줄이는 피해 비율. 0.5는 절반, 1은 무적")]
        [SerializeField, Range(0f, 1f)] private float damageReduction = 0.5f;
        [Tooltip("지속 시간(초). 0이면 계속")]
        [SerializeField, Min(0f)] private float duration = 5f;

        public float DamageReduction => damageReduction;
        public float Duration => duration;

        public override EnemyAbilityRuntime CreateRuntime(Enemy owner)
        {
            return new Runtime(this, owner);
        }

        private sealed class Runtime : EnemyAbilityRuntime
        {
            private readonly ShieldAbility data;
            private float remaining;
            private bool shielding;

            public Runtime(ShieldAbility data, Enemy owner) : base(data, owner)
            {
                this.data = data;
            }

            protected override void OnActivated()
            {
                Owner.Health.AddDamageReduction(data.DamageReduction);
                remaining = data.Duration;
                shielding = true;
            }

            protected override void OnTick(float deltaTime)
            {
                if (!shielding || data.Duration <= 0f) return;

                remaining -= deltaTime;
                if (remaining > 0f) return;

                Owner.Health.RemoveDamageReduction(data.DamageReduction);
                shielding = false;
            }
        }
    }
}
