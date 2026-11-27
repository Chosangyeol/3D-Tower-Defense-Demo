using TowerDefense.Effects;
using TowerDefense.Enemies;
using TowerDefense.Game;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Economy
{
    /// <summary>
    /// 적 처치 보상과 웨이브 완료 보상을 창고에 넣는다. 적·웨이브 데이터에 웨이브 배율과 효과를 적용한다.
    /// </summary>
    public class RewardDispenser : MonoBehaviour
    {
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunModifiers modifiers;

        private void OnEnable()
        {
            waveSpawner.EnemyKilled += OnEnemyKilled;
            waveSpawner.WaveCleared += OnWaveCleared;
        }

        private void OnDisable()
        {
            waveSpawner.EnemyKilled -= OnEnemyKilled;
            waveSpawner.WaveCleared -= OnWaveCleared;
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            wallet.Add(modifiers.Scale(enemy.Definition.Reward, Stat.KillReward, enemy.Stats.RewardScale, enemy.Definition));
        }

        /// <param name="wave">1부터 센 웨이브 번호</param>
        private void OnWaveCleared(int wave)
        {
            WaveDefinition definition = setup.Campaign.Waves[wave - 1];
            if (definition == null) return;

            float multiplier = setup.Campaign.GetScale(wave).Reward;
            wallet.Add(modifiers.Scale(definition.ClearReward, Stat.WaveReward, multiplier));
        }
    }
}
