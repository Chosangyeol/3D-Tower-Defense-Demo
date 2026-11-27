using System.Collections.Generic;
using TowerDefense.Augments;
using TowerDefense.Data;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>한 웨이브의 강도 배율. 1이 데이터 그대로다.</summary>
    public readonly struct WaveScale
    {
        public WaveScale(float count, float health, float attack, float reward)
        {
            Count = count;
            Health = health;
            Attack = attack;
            Reward = reward;
        }

        public float Count { get; }
        public float Health { get; }
        public float Attack { get; }
        public float Reward { get; }
    }

    /// <summary>
    /// 한 판의 진행: 웨이브 순서, 준비 시간, 웨이브가 갈수록 강해지는 정도, 선택할 수 있는 제약, 증강.
    /// 마지막 웨이브를 막으면 클리어다.
    /// </summary>
    [GameDataCategory("진행", "Campaigns", 45)]
    [CreateAssetMenu(menuName = "Tower Defense/Campaign Definition", fileName = "Campaign")]
    public class CampaignDefinition : GameData
    {
        [Header("준비 시간(초)")]
        [Tooltip("게임 시작부터 첫 웨이브까지. 마을·타워 배치를 고려해 넉넉하게 둔다")]
        [SerializeField, Min(1f)] private float firstWaveDelay = 180f;
        [Tooltip("웨이브가 끝난 뒤 다음 웨이브까지")]
        [SerializeField, Min(1f)] private float waveInterval = 60f;

        [Header("웨이브마다 늘어나는 비율 (0.05 = 웨이브당 +5%)")]
        [Tooltip("보스의 수는 늘리지 않는다")]
        [SerializeField, Min(0f)] private float countPerWave = 0.01f;
        [SerializeField, Min(0f)] private float healthPerWave = 0.05f;
        [SerializeField, Min(0f)] private float attackPerWave = 0.01f;
        [SerializeField, Min(0f)] private float rewardPerWave = 0.02f;

        [Header("웨이브 (순서대로)")]
        [SerializeField] private WaveDefinition[] waves = new WaveDefinition[0];

        [Header("클리어한 뒤 선택할 수 있는 제약")]
        [SerializeField] private ConstraintDefinition[] constraints = new ConstraintDefinition[0];

        [Header("증강")]
        [Tooltip("여기에 넣은 획득 방식만 켜진다")]
        [SerializeField] private AugmentSource[] augmentSources = new AugmentSource[0];
        [Tooltip("후보로 나오는 증강")]
        [SerializeField] private AugmentDefinition[] augments = new AugmentDefinition[0];
        [Tooltip("한 번에 보여주는 후보 수")]
        [SerializeField, Min(1)] private int augmentChoices = 3;
        [Tooltip("한 판에서 후보를 다시 뽑을 수 있는 횟수")]
        [SerializeField, Min(0)] private int augmentRerolls = 2;

        public IReadOnlyList<AugmentSource> AugmentSources => augmentSources;
        public IReadOnlyList<AugmentDefinition> Augments => augments;
        public int AugmentChoices => augmentChoices;
        public int AugmentRerolls => augmentRerolls;
        public float FirstWaveDelay => firstWaveDelay;
        public float WaveInterval => waveInterval;
        public IReadOnlyList<WaveDefinition> Waves => waves;
        public IReadOnlyList<ConstraintDefinition> Constraints => constraints;

        /// <param name="wave">1부터 센 웨이브 번호</param>
        public WaveScale GetScale(int wave)
        {
            int steps = Mathf.Max(wave - 1, 0);
            return new WaveScale(
                1f + countPerWave * steps,
                1f + healthPerWave * steps,
                1f + attackPerWave * steps,
                1f + rewardPerWave * steps);
        }
    }
}
