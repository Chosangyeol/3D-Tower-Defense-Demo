using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using TowerDefense.Upgrades;
using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 맵 하나의 구성: 기지, 시작 자원, 연구, 적 경로.
    /// </summary>
    [GameDataCategory("맵", "Maps", 50)]
    [CreateAssetMenu(menuName = "Tower Defense/Map Definition", fileName = "Map")]
    public class MapDefinition : GameData
    {
        [Header("기지")]
        [SerializeField, Min(1)] private int baseHealth = 100;

        [Header("자원")]
        [Tooltip("시작할 때 지급. 여기 적은 순서대로 화면에 표시한다 (0개도 표시)")]
        [SerializeField] private ResourceAmount[] startResources = new ResourceAmount[0];
        [Tooltip("웨이브 진행 중에 타워를 제거하면 돌려받는 비율. 준비 시간에는 전액")]
        [SerializeField, Range(0f, 1f)] private float combatRefundRate = 0.5f;

        [Header("연구 (연구 화면에 보이는 순서)")]
        [SerializeField] private UpgradeDefinition[] upgrades = new UpgradeDefinition[0];

        [Header("적 경로")]
        [Tooltip("꺾이는 지점의 그리드 칸 좌표. 가로·세로 직선만 가능하고, 마지막 칸에 기지가 놓인다")]
        [SerializeField] private Vector2Int[] pathWaypoints = new Vector2Int[0];

        public int BaseHealth => baseHealth;
        public IReadOnlyList<ResourceAmount> StartResources => startResources;
        public float CombatRefundRate => combatRefundRate;
        public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades;
        public IReadOnlyList<Vector2Int> PathWaypoints => pathWaypoints;
    }
}
