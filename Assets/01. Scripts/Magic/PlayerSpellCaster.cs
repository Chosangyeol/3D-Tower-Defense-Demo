using TowerDefense.Controls;
using TowerDefense.Game;
using UnityEngine;

namespace TowerDefense.Magic
{
    /// <summary>
    /// 전투 모드에서 마법을 시전한다. 버튼을 누르고 있으면 일정 간격으로 계속 쏘며, 마나를 소모한다.
    /// </summary>
    [RequireComponent(typeof(PlayerMana))]
    public class PlayerSpellCaster : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private Transform head;
        [SerializeField] private SpellProjectile projectilePrefab;

        [Header("수치 (플레이 테스트 후 조정)")]
        [SerializeField, Min(0f)] private float manaCost = 10f;
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.05f)] private float castInterval = 0.35f;
        [SerializeField, Min(1f)] private float projectileSpeed = 35f;

        private PlayerMana mana;
        private float nextCastTime;

        public float ManaCost => manaCost;

        private void Awake()
        {
            mana = GetComponent<PlayerMana>();
        }

        private void Update()
        {
            if (modeController.CurrentMode != GameMode.Combat) return;
            // 커서가 풀린 상태의 클릭(창 선택 등)은 시전으로 보지 않는다
            if (Cursor.lockState != CursorLockMode.Locked) return;

            if (input.Cast.IsPressed()) TryCast();
        }

        /// <summary>시전 간격이 지났고 마나가 충분하면 조준 방향으로 투사체를 쏜다.</summary>
        public bool TryCast()
        {
            if (projectilePrefab == null || Time.time < nextCastTime || !mana.TrySpend(manaCost)) return false;

            nextCastTime = Time.time + castInterval;
            SpellProjectile projectile = Instantiate(projectilePrefab, head.position, head.rotation);
            projectile.Initialize(transform, damage, projectileSpeed);
            return true;
        }
    }
}
