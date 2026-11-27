using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 타워 종류별 데이터. 수치는 플레이 테스트 후 조정한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Tower Defense/Tower Definition", fileName = "Tower")]
    public class TowerDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "타워";
        [SerializeField] private Tower prefab;
        [SerializeField, Min(0f)] private float range = 6f;

        public string DisplayName => displayName;
        public Tower Prefab => prefab;
        public float Range => range;
    }
}
