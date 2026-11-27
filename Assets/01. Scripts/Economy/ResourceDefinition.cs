using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Economy
{
    /// <summary>
    /// 자원 종류. (목재, 광석, 마력 결정)
    /// </summary>
    [GameDataCategory("자원", "ResourceTypes", 5)]
    [CreateAssetMenu(menuName = "Tower Defense/Resource", fileName = "Resource")]
    public class ResourceDefinition : GameData
    {
        [Tooltip("화면에 표시할 색")]
        [SerializeField] private Color color = Color.white;

        public Color Color => color;
    }
}
