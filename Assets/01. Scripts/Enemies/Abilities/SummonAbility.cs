using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>일정한 주기로 부하를 부른다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Enemy Ability/Summon", fileName = "SummonAbility")]
    public class SummonAbility : EnemyAbility
    {
        [SerializeField] private EnemyDefinition summoned;
        [SerializeField, Min(1)] private int count = 3;
        [Tooltip("소환 주기(초). 발동하고 한 주기가 지난 뒤에 처음 부른다")]
        [SerializeField, Min(0.5f)] private float interval = 10f;

        public EnemyDefinition Summoned => summoned;
        public int Count => count;
        public float Interval => interval;

        public override EnemyAbilityRuntime CreateRuntime(Enemy owner)
        {
            return new Runtime(this, owner);
        }

        private sealed class Runtime : EnemyAbilityRuntime
        {
            private readonly SummonAbility data;
            private float timer;

            public Runtime(SummonAbility data, Enemy owner) : base(data, owner)
            {
                this.data = data;
            }

            protected override void OnTick(float deltaTime)
            {
                timer += deltaTime;
                if (timer < data.Interval) return;

                timer -= data.Interval;
                Owner.RequestSummon(data.Summoned, data.Count);
            }
        }
    }
}
