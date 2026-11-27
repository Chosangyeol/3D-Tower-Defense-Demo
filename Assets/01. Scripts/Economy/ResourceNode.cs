using TowerDefense.Grid;
using UnityEngine;

namespace TowerDefense.Economy
{
    /// <summary>
    /// 맵에 놓인 자원 노드. 매장량이 떨어지면 시간이 지나 다시 채워진다.
    /// 노드가 놓인 칸에는 타워를 설치할 수 없다. 씬에서는 칸의 중심에 놓는다.
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        private const float DepletedHeight = 0.2f;

        [SerializeField] private ResourceNodeDefinition definition;
        [SerializeField] private BuildGrid grid;
        [Tooltip("고갈되면 납작해지는 모형")]
        [SerializeField] private Transform visual;

        private Vector3 fullScale;
        private Vector3 fullPosition;

        public ResourceNodeDefinition Definition => definition;
        public int Remaining { get; private set; }
        public bool IsAvailable => Remaining > 0;
        /// <summary>다시 채워지기까지 남은 시간(초). 고갈되지 않았으면 0.</summary>
        public float TimeUntilRespawn { get; private set; }

        private void Awake()
        {
            Remaining = definition.Capacity;
            if (visual != null)
            {
                fullScale = visual.localScale;
                fullPosition = visual.localPosition;
            }
        }

        private void Start()
        {
            Vector2Int cell = grid.WorldToCell(transform.position);
            if (!grid.Block(cell)) Debug.LogError($"자원 노드의 칸을 막을 수 없습니다: {cell}", this);
        }

        private void Update()
        {
            if (IsAvailable) return;

            TimeUntilRespawn = Mathf.Max(TimeUntilRespawn - Time.deltaTime, 0f);
            if (TimeUntilRespawn > 0f) return;

            Remaining = definition.Capacity;
            ShowDepleted(false);
        }

        /// <summary>한 번 채집하고 얻은 양을 돌려준다. 고갈 상태면 0.</summary>
        public int Gather()
        {
            if (!IsAvailable) return 0;

            int amount = Mathf.Min(definition.AmountPerGather, Remaining);
            Remaining -= amount;
            if (!IsAvailable)
            {
                TimeUntilRespawn = definition.RespawnSeconds;
                ShowDepleted(true);
            }
            return amount;
        }

        private void ShowDepleted(bool depleted)
        {
            if (visual == null) return;

            float height = depleted ? DepletedHeight : 1f;
            visual.localScale = Vector3.Scale(fullScale, new Vector3(1f, height, 1f));
            visual.localPosition = Vector3.Scale(fullPosition, new Vector3(1f, height, 1f));
        }
    }
}
