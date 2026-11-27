using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 한 맵(스테이지)의 구성: 기지, 준비 시간, 웨이브 순서, 적 경로.
    /// </summary>
    [GameDataCategory("스테이지", "Stages", 50)]
    [CreateAssetMenu(menuName = "Tower Defense/Stage Definition", fileName = "Stage")]
    public class StageDefinition : GameData
    {
        [Header("기지")]
        [SerializeField, Min(1)] private int baseHealth = 10;

        [Header("준비 시간(초)")]
        [Tooltip("게임 시작부터 첫 웨이브까지. 마을·타워 배치를 고려해 넉넉하게 둔다")]
        [SerializeField, Min(1f)] private float firstWaveDelay = 180f;
        [Tooltip("웨이브가 끝난 뒤 다음 웨이브까지")]
        [SerializeField, Min(1f)] private float waveInterval = 60f;

        [Header("웨이브 (순서대로)")]
        [SerializeField] private WaveDefinition[] waves = new WaveDefinition[0];

        [Header("적 경로")]
        [Tooltip("꺾이는 지점의 그리드 칸 좌표. 가로·세로 직선만 가능하고, 마지막 칸에 기지가 놓인다")]
        [SerializeField] private Vector2Int[] pathWaypoints = new Vector2Int[0];

        public int BaseHealth => baseHealth;
        public float FirstWaveDelay => firstWaveDelay;
        public float WaveInterval => waveInterval;
        public IReadOnlyList<WaveDefinition> Waves => waves;
        public IReadOnlyList<Vector2Int> PathWaypoints => pathWaypoints;
    }
}
