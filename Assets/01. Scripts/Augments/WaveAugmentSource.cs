using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Augments
{
    /// <summary>정해진 웨이브를 막으면 증강을 고른다. 마지막 웨이브는 막는 순간 클리어라 제외한다.</summary>
    [CreateAssetMenu(menuName = "Tower Defense/Augment Source/Wave", fileName = "WaveAugmentSource")]
    public class WaveAugmentSource : AugmentSource
    {
        [Tooltip("이 번호의 웨이브를 막으면 증강이 나온다")]
        [SerializeField] private int[] waves = new int[0];

        public IReadOnlyList<int> Waves => waves;

        public override AugmentSourceRuntime CreateRuntime(AugmentSystem system)
        {
            return new Runtime(this, system);
        }

        private sealed class Runtime : AugmentSourceRuntime
        {
            private readonly WaveAugmentSource data;

            public Runtime(WaveAugmentSource data, AugmentSystem system) : base(system)
            {
                this.data = data;
            }

            public override void OnWaveCleared(int wave)
            {
                if (!System.IsFinalWave && Array.IndexOf(data.waves, wave) >= 0) System.RequestOffer(data.Tier);
            }
        }
    }
}
