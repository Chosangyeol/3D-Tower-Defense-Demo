using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using TowerDefense.Effects;
using UnityEngine;

namespace TowerDefense.Structures
{
    /// <summary>
    /// 그리드 칸에 설치하는 것(타워, 건물)의 공통 데이터. 상속하면 하단 설치 목록에 넣어 설치할 수 있다.
    /// </summary>
    public abstract class StructureDefinition : GameData
    {
        [Tooltip("설치 비용")]
        [SerializeField] private ResourceAmount[] cost = new ResourceAmount[0];

        /// <summary>설치하면 만들어지는 프리팹. 설치 전 미리보기에도 쓴다.</summary>
        public abstract Structure Prefab { get; }
        /// <summary>효과를 적용하기 전의 설치 비용.</summary>
        public IReadOnlyList<ResourceAmount> Cost => cost;

        /// <summary>지금 설치할 때 내야 하는 비용.</summary>
        public virtual List<ResourceAmount> GetCost(RunModifiers modifiers)
        {
            return new List<ResourceAmount>(cost);
        }

        /// <summary>설치 미리보기에 보여줄 범위 반경. 범위가 없으면 0.</summary>
        public virtual float GetRange(RunModifiers modifiers)
        {
            return 0f;
        }
    }
}
