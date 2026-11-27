using TowerDefense.Augments;
using TowerDefense.Controls;
using TowerDefense.Game;
using UnityEngine;

namespace TowerDefense.Player
{
    /// <summary>
    /// 1인칭에서 제단 사용. 조준한 제단이 가까이에 있을 때 채집 키를 길게 누르면 증강을 산다.
    /// 한 번 산 뒤에는 키를 뗐다가 다시 눌러야 한다.
    /// </summary>
    public class PlayerAltarUser : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private Transform head;
        [SerializeField, Min(0.5f)] private float reach = 3.5f;

        private float elapsed;
        private bool waitingRelease;

        /// <summary>조준 중인 제단. 없으면 null.</summary>
        public AugmentAltar Target { get; private set; }
        /// <summary>지금 구매의 진행도 (0~1).</summary>
        public float Progress => Target != null ? Mathf.Clamp01(elapsed / Target.HoldSeconds) : 0f;

        private void OnDisable()
        {
            Target = null;
            elapsed = 0f;
        }

        private void Update()
        {
            // 게임이 멈춘 동안에는 상태를 그대로 둔다
            if (Time.deltaTime <= 0f) return;

            AugmentAltar found = modeController.CurrentMode == GameMode.Combat ? FindTarget() : null;
            if (found != Target)
            {
                Target = found;
                elapsed = 0f;
            }

            if (!input.Interact.IsPressed())
            {
                waitingRelease = false;
                elapsed = 0f;
                return;
            }

            if (Target == null || waitingRelease || !Target.CanPurchase)
            {
                elapsed = 0f;
                return;
            }

            elapsed += Time.deltaTime;
            if (elapsed < Target.HoldSeconds) return;

            elapsed = 0f;
            waitingRelease = true;
            Target.TryPurchase();
        }

        private AugmentAltar FindTarget()
        {
            // 레이가 머리(플레이어 콜라이더 안)에서 출발하므로 자신은 맞지 않는다
            bool hit = Physics.Raycast(head.position, head.forward, out RaycastHit info, reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            return hit ? info.collider.GetComponentInParent<AugmentAltar>() : null;
        }
    }
}
