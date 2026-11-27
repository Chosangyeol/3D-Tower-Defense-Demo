using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Player
{
    /// <summary>
    /// 플레이어의 기본 능력치. 시점 감도 같은 개인 설정은 넣지 않는다.
    /// 실행 중에 값을 바꾸지 않는다. (강화는 별도의 계산 결과로 다룬다)
    /// </summary>
    [GameDataCategory("플레이어 능력치", "Players", 30)]
    [CreateAssetMenu(menuName = "Tower Defense/Player Stats", fileName = "PlayerStats")]
    public class PlayerStatsDefinition : GameData
    {
        [Header("이동")]
        [SerializeField, Min(0f)] private float walkSpeed = 5f;
        [SerializeField, Min(0f)] private float sprintSpeed = 8f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("마나")]
        [SerializeField, Min(1f)] private float maxMana = 100f;
        [SerializeField, Min(0f)] private float manaRegenPerSecond = 12f;

        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float MaxMana => maxMana;
        public float ManaRegenPerSecond => manaRegenPerSecond;
    }
}
