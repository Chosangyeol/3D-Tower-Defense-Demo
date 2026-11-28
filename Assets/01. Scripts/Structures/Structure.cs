using System.Collections.Generic;
using TowerDefense.Economy;
using TowerDefense.Effects;
using UnityEngine;

namespace TowerDefense.Structures
{
    /// <summary>
    /// 그리드 칸에 설치된 것(타워, 건물). 선택하면 이름과 상태를 보여주고 제거할 수 있다.
    /// </summary>
    public abstract class Structure : MonoBehaviour
    {
        public StructureDefinition Definition { get; private set; }
        public Vector2Int Cell { get; private set; }
        /// <summary>설치할 때 낸 비용. 환급은 이 값을 기준으로 한다.</summary>
        public IReadOnlyList<ResourceAmount> PaidCost { get; private set; }
        /// <summary>선택했을 때 보여줄 범위 반경. 범위가 없으면 0.</summary>
        public virtual float Range => 0f;

        protected RunModifiers Modifiers { get; private set; }

        public void Initialize(StructureDefinition definition, Vector2Int cell, RunModifiers modifiers,
            IReadOnlyList<ResourceAmount> paidCost)
        {
            Definition = definition;
            Cell = cell;
            Modifiers = modifiers;
            PaidCost = paidCost;
        }

        /// <summary>선택했을 때 보여줄 지금 상태.</summary>
        public virtual string Describe()
        {
            return "";
        }
    }
}
