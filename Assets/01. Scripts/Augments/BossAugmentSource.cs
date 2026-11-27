using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Augments
{
    /// <summary>보스를 처치하면 바로 증강을 고른다. 마지막 웨이브의 보스는 제외한다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Augment Source/Boss", fileName = "BossAugmentSource")]
    public class BossAugmentSource : AugmentSource
    {
        public override AugmentSourceRuntime CreateRuntime(AugmentSystem system)
        {
            return new Runtime(this, system);
        }

        private sealed class Runtime : AugmentSourceRuntime
        {
            private readonly BossAugmentSource data;

            public Runtime(BossAugmentSource data, AugmentSystem system) : base(system)
            {
                this.data = data;
            }

            public override void OnEnemyKilled(Enemy enemy)
            {
                if (enemy.Definition.IsBoss && !System.IsFinalWave) System.RequestOffer(data.Tier);
            }
        }
    }
}
