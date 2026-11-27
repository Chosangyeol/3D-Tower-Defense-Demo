using TowerDefense.Controls;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Game;
using UnityEngine;

namespace TowerDefense.Player
{
    /// <summary>
    /// 1인칭 채집. 조준한 자원 노드가 가까이에 있을 때 채집 키를 길게 누르면 자원을 얻는다.
    /// </summary>
    public class PlayerGatherer : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private Transform head;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private RunModifiers modifiers;
        [SerializeField, Min(0.5f)] private float reach = 3.5f;

        private float elapsed;

        /// <summary>조준 중인 자원 노드. 없으면 null.</summary>
        public ResourceNode Target { get; private set; }
        /// <summary>지금 채집의 진행도 (0~1).</summary>
        public float Progress => Target != null ? Mathf.Clamp01(elapsed / GatherSeconds) : 0f;

        private float GatherSeconds =>
            modifiers.Get(Stat.GatherSeconds, Target.Definition.GatherSeconds, Target.Definition);

        private void OnDisable()
        {
            Target = null;
            elapsed = 0f;
        }

        private void Update()
        {
            // 게임이 멈춘 동안에는 채집 진행도를 그대로 둔다
            if (Time.deltaTime <= 0f) return;

            ResourceNode found = modeController.CurrentMode == GameMode.Combat ? FindTarget() : null;
            if (found != Target)
            {
                Target = found;
                elapsed = 0f;
            }

            if (Target == null || !Target.IsAvailable || !input.Interact.IsPressed())
            {
                elapsed = 0f;
                return;
            }

            elapsed += Time.deltaTime;
            if (elapsed < GatherSeconds) return;

            elapsed = 0f;
            int gathered = modifiers.GetInt(Stat.GatherAmount, Target.Gather(), Target.Definition);
            wallet.Add(Target.Definition.Resource, gathered);
        }

        private ResourceNode FindTarget()
        {
            // 레이가 머리(플레이어 콜라이더 안)에서 출발하므로 자신은 맞지 않는다
            bool hit = Physics.Raycast(head.position, head.forward, out RaycastHit info, reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            return hit ? info.collider.GetComponentInParent<ResourceNode>() : null;
        }
    }
}
