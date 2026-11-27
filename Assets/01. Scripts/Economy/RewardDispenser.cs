using TowerDefense.Enemies;
using TowerDefense.Game;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Economy
{
    /// <summary>
    /// 적 처치 보상과 웨이브 완료 보상을 창고에 넣는다. 보상은 적과 웨이브 데이터를 따른다.
    /// </summary>
    public class RewardDispenser : MonoBehaviour
    {
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private StageController stageController;

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
            wallet.Add(enemy.Definition.Reward);
        }

        /// <param name="wave">1부터 센 웨이브 번호</param>
        private void OnWaveCleared(int wave)
        {
            WaveDefinition definition = stageController.Stage.Waves[wave - 1];
            if (definition != null) wallet.Add(definition.ClearReward);
        }
    }
}
