using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// 적 종류별 데이터. 수치는 플레이 테스트 후 조정한다.
    /// </summary>
    [GameDataCategory("적", "Enemies", 10)]
    [CreateAssetMenu(menuName = "Tower Defense/Enemy Definition", fileName = "Enemy")]
    public class EnemyDefinition : GameData
    {
        [SerializeField] private Enemy prefab;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
        [SerializeField, Min(1f)] private float maxHealth = 50f;
        [Tooltip("기지에 도달했을 때 줄이는 기지 체력")]
        [SerializeField, Min(1)] private int baseDamage = 1;

        public Enemy Prefab => prefab;
        public float MoveSpeed => moveSpeed;
        public float MaxHealth => maxHealth;
        public int BaseDamage => baseDamage;
    }
}
