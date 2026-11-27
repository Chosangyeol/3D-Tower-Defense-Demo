using System;
using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Waves
{
    /// <summary>같은 종류의 적을 일정 간격으로 내보내는 묶음.</summary>
    [Serializable]
    public struct SpawnGroup
    {
        public EnemyDefinition enemy;
        [Min(1)] public int count;
        [Min(0.05f)] public float interval;
    }

    /// <summary>
    /// 웨이브 하나. 묶음을 순서대로 내보낸다.
    /// </summary>
    [GameDataCategory("웨이브", "Waves", 40)]
    [CreateAssetMenu(menuName = "Tower Defense/Wave Definition", fileName = "Wave")]
    public class WaveDefinition : GameData
    {
        [SerializeField] private SpawnGroup[] groups = new SpawnGroup[0];

        [Tooltip("웨이브를 막았을 때 얻는 자원")]
        [SerializeField] private ResourceAmount[] clearReward = new ResourceAmount[0];

        public IReadOnlyList<SpawnGroup> Groups => groups;
        public IReadOnlyList<ResourceAmount> ClearReward => clearReward;
    }
}
