using System.Collections.Generic;
using TowerDefense.Economy;
using UnityEngine;

namespace TowerDefense.Augments
{
    /// <summary>
    /// 맵의 제단에서 자원을 내고 증강을 산다. 살 때마다 비용이 늘어난다.
    /// 이 방식이 목록에 있어야 맵의 제단이 나타난다.
    /// </summary>
    [CreateAssetMenu(menuName = "Tower Defense/Augment Source/Altar", fileName = "AltarAugmentSource")]
    public class AltarAugmentSource : AugmentSource
    {
        [Tooltip("첫 번째 구매 비용")]
        [SerializeField] private ResourceAmount[] cost = new ResourceAmount[0];
        [Tooltip("한 번 살 때마다 늘어나는 비용")]
        [SerializeField] private ResourceAmount[] costIncrease = new ResourceAmount[0];
        [Tooltip("채집 키를 누르고 있어야 하는 시간(초)")]
        [SerializeField, Min(0.1f)] private float holdSeconds = 1.5f;

        public IReadOnlyList<ResourceAmount> Cost => cost;
        public IReadOnlyList<ResourceAmount> CostIncrease => costIncrease;
        public float HoldSeconds => holdSeconds;

        public override AugmentSourceRuntime CreateRuntime(AugmentSystem system)
        {
            return new AltarSourceRuntime(this, system);
        }
    }

    /// <summary>제단 구매의 상태: 산 횟수와 지금 비용.</summary>
    public sealed class AltarSourceRuntime : AugmentSourceRuntime
    {
        private readonly AltarAugmentSource data;

        public AltarSourceRuntime(AltarAugmentSource data, AugmentSystem system) : base(system)
        {
            this.data = data;
        }

        public int Purchases { get; private set; }
        public float HoldSeconds => data.HoldSeconds;
        /// <summary>고를 수 있는 증강이 남아 있는지 여부.</summary>
        public bool HasStock => System.HasEligible(data.TierChances);
        public bool CanPurchase => HasStock && !System.HasWaitingOffer && System.Wallet.CanAfford(GetCost());

        /// <summary>지금 사는 데 드는 비용.</summary>
        public List<ResourceAmount> GetCost()
        {
            var total = new List<ResourceAmount>(data.Cost);
            foreach (ResourceAmount step in data.CostIncrease)
            {
                int added = step.amount * Purchases;
                int index = total.FindIndex(item => item.resource == step.resource);
                if (index >= 0) total[index] = new ResourceAmount(step.resource, total[index].amount + added);
                else if (added > 0) total.Add(new ResourceAmount(step.resource, added));
            }
            return total;
        }

        /// <summary>비용을 내고 증강 선택을 요청한다.</summary>
        public bool TryPurchase()
        {
            if (!CanPurchase || !System.Wallet.TrySpend(GetCost())) return false;

            Purchases++;
            return System.RequestOffer(data.TierChances);
        }
    }
}
