using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Economy
{
    /// <summary>
    /// 자원 노드 종류: 무엇을, 얼마나 빨리, 얼마나 얻는지.
    /// </summary>
    [GameDataCategory("자원 노드", "ResourceNodes", 6)]
    [CreateAssetMenu(menuName = "Tower Defense/Resource Node", fileName = "ResourceNode")]
    public class ResourceNodeDefinition : GameData
    {
        [SerializeField] private ResourceDefinition resource;
        [Tooltip("한 번 채집해서 얻는 양")]
        [SerializeField, Min(1)] private int amountPerGather = 5;
        [Tooltip("한 번 채집하는 데 걸리는 시간(초)")]
        [SerializeField, Min(0.1f)] private float gatherSeconds = 1.5f;
        [Tooltip("고갈되기 전까지 얻을 수 있는 총량")]
        [SerializeField, Min(1)] private int capacity = 40;
        [Tooltip("고갈된 뒤 다시 채워지기까지의 시간(초)")]
        [SerializeField, Min(1f)] private float respawnSeconds = 45f;

        public ResourceDefinition Resource => resource;
        public int AmountPerGather => amountPerGather;
        public float GatherSeconds => gatherSeconds;
        public int Capacity => capacity;
        public float RespawnSeconds => respawnSeconds;
    }
}
