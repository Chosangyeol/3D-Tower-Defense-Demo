using System.Collections.Generic;
using TowerDefense.Economy;
using TowerDefense.Grid;
using UnityEngine;

namespace TowerDefense.Augments
{
    /// <summary>
    /// 맵에 놓인 증강 제단. 진행 데이터에 제단 구매 방식이 없으면 사라진다.
    /// 제단이 놓인 칸에는 타워를 설치할 수 없다. 씬에서는 칸의 중심에 놓는다.
    /// </summary>
    public class AugmentAltar : MonoBehaviour
    {
        [SerializeField] private AugmentSystem augments;
        [SerializeField] private BuildGrid grid;

        private AltarSourceRuntime source;

        public float HoldSeconds => source.HoldSeconds;
        public bool HasStock => source.HasStock;
        public bool CanPurchase => source.CanPurchase;

        public List<ResourceAmount> GetCost()
        {
            return source.GetCost();
        }

        public bool TryPurchase()
        {
            return source.TryPurchase();
        }

        // 획득 방식은 AugmentSystem의 Awake에서 만들어지므로 Start에서 확인한다
        private void Start()
        {
            source = augments.FindSource<AltarSourceRuntime>();
            if (source == null)
            {
                gameObject.SetActive(false);
                return;
            }

            Vector2Int cell = grid.WorldToCell(transform.position);
            if (!grid.Block(cell)) Debug.LogError($"제단의 칸을 막을 수 없습니다: {cell}", this);
        }
    }
}
