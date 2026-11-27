using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>발동하면 이동 속도가 계속 바뀐다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Enemy Ability/Speed", fileName = "SpeedAbility")]
    public class SpeedAbility : EnemyAbility
    {
        [Tooltip("속도 비율. 0.3은 +30%, -0.2는 -20%")]
        [SerializeField] private float percent = 0.3f;

        public float Percent => percent;

        public override EnemyAbilityRuntime CreateRuntime(Enemy owner)
        {
            return new Runtime(this, owner);
        }

        private sealed class Runtime : EnemyAbilityRuntime
        {
            private readonly SpeedAbility data;

            public Runtime(SpeedAbility data, Enemy owner) : base(data, owner)
            {
                this.data = data;
            }

            protected override void OnActivated()
            {
                Owner.AddSpeedBonus(data.Percent);
            }
        }
    }
}
