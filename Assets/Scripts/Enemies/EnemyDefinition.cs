using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// 적 종류별 데이터. 체력은 타워 공격을 만들 때 추가한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Tower Defense/Enemy Definition", fileName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "적";
        [SerializeField] private Enemy prefab;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;

        public string DisplayName => displayName;
        public Enemy Prefab => prefab;
        public float MoveSpeed => moveSpeed;
    }
}
