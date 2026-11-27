using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TowerDefense.Economy
{
    /// <summary>자원 종류와 수량. 비용, 보상, 시작 자원에 사용한다.</summary>
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceDefinition resource;
        [Min(0)] public int amount;

        public ResourceAmount(ResourceDefinition resource, int amount)
        {
            this.resource = resource;
            this.amount = amount;
        }

        /// <summary>"목재 30, 광석 10" 형태의 글자. 비어 있으면 "없음".</summary>
        public static string Describe(IReadOnlyList<ResourceAmount> amounts)
        {
            var text = new StringBuilder();
            foreach (ResourceAmount item in amounts)
            {
                if (item.resource == null || item.amount <= 0) continue;

                if (text.Length > 0) text.Append(", ");
                text.Append(item.resource.DisplayName).Append(' ').Append(item.amount);
            }
            return text.Length > 0 ? text.ToString() : "없음";
        }
    }
}
