using System;
using System.Collections.Generic;
using TowerDefense.Effects;
using TowerDefense.Game;
using UnityEngine;

namespace TowerDefense.Economy
{
    /// <summary>
    /// 공용 창고. 보유한 자원을 관리한다. 시작 자원은 맵 데이터를 따른다.
    /// </summary>
    [DefaultExecutionOrder(-120)]
    public class ResourceWallet : MonoBehaviour
    {
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunModifiers modifiers;

        private readonly Dictionary<ResourceDefinition, int> amounts = new Dictionary<ResourceDefinition, int>();
        private readonly List<ResourceDefinition> kinds = new List<ResourceDefinition>();

        /// <summary>보유량이 바뀔 때 호출된다.</summary>
        public event Action Changed;

        /// <summary>화면에 보여줄 자원 목록. 시작 자원에 적은 순서를 따른다.</summary>
        public IReadOnlyList<ResourceDefinition> Kinds => kinds;

        private void Awake()
        {
            foreach (ResourceAmount start in modifiers.Scale(setup.Map.StartResources, Stat.StartResources))
            {
                Add(start.resource, start.amount);
            }
        }

        public int GetAmount(ResourceDefinition resource)
        {
            return resource != null && amounts.TryGetValue(resource, out int amount) ? amount : 0;
        }

        public void Add(ResourceDefinition resource, int amount)
        {
            if (resource == null || amount < 0) return;

            if (!amounts.ContainsKey(resource)) kinds.Add(resource);
            amounts[resource] = GetAmount(resource) + amount;
            Changed?.Invoke();
        }

        public void Add(IReadOnlyList<ResourceAmount> rewards)
        {
            foreach (ResourceAmount reward in rewards)
            {
                Add(reward.resource, reward.amount);
            }
        }

        public bool CanAfford(IReadOnlyList<ResourceAmount> cost)
        {
            foreach (KeyValuePair<ResourceDefinition, int> total in Sum(cost))
            {
                if (GetAmount(total.Key) < total.Value) return false;
            }
            return true;
        }

        /// <summary>모든 자원이 충분할 때만 소모한다.</summary>
        public bool TrySpend(IReadOnlyList<ResourceAmount> cost)
        {
            if (!CanAfford(cost)) return false;

            foreach (KeyValuePair<ResourceDefinition, int> total in Sum(cost))
            {
                amounts[total.Key] = GetAmount(total.Key) - total.Value;
            }
            Changed?.Invoke();
            return true;
        }

        // 같은 자원이 여러 번 적혀 있어도 합쳐서 계산한다
        private static Dictionary<ResourceDefinition, int> Sum(IReadOnlyList<ResourceAmount> cost)
        {
            var totals = new Dictionary<ResourceDefinition, int>();
            foreach (ResourceAmount item in cost)
            {
                if (item.resource == null || item.amount <= 0) continue;

                totals.TryGetValue(item.resource, out int current);
                totals[item.resource] = current + item.amount;
            }
            return totals;
        }
    }
}
