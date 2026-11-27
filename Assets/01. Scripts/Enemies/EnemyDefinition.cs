using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using UnityEngine;
using UnityEngine.Serialization;

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

        [Header("기지 공격")]
        [FormerlySerializedAs("baseDamage")]
        [Tooltip("기지를 한 번 공격할 때의 피해")]
        [SerializeField, Min(1)] private int attackDamage = 5;
        [Tooltip("공격 간격(초)")]
        [SerializeField, Min(0.1f)] private float attackInterval = 2f;

        [Header("특수")]
        [Tooltip("보스는 화면 위에 체력 막대를 표시한다")]
        [SerializeField] private bool isBoss;
        [SerializeField] private EnemyAbility[] abilities = new EnemyAbility[0];

        [Tooltip("처치했을 때 얻는 자원")]
        [SerializeField] private ResourceAmount[] reward = new ResourceAmount[0];

        public Enemy Prefab => prefab;
        public float MoveSpeed => moveSpeed;
        public float MaxHealth => maxHealth;
        public int AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public bool IsBoss => isBoss;
        public IReadOnlyList<EnemyAbility> Abilities => abilities;
        public IReadOnlyList<ResourceAmount> Reward => reward;
    }
}
