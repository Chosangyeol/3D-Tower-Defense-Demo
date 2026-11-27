using System.Collections.Generic;
using System.Linq;
using TowerDefense.Effects;
using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 이번 판에 걸린 제약. 판이 시작될 때 선택해 둔 제약의 효과를 적용하며, 판 중간에는 바뀌지 않는다.
    /// </summary>
    // 다른 컴포넌트가 시작 값을 읽기 전에 적용한다
    [DefaultExecutionOrder(-155)]
    public class RunConstraints : MonoBehaviour
    {
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunModifiers modifiers;

        private List<ConstraintDefinition> active = new List<ConstraintDefinition>();

        public IReadOnlyList<ConstraintDefinition> Active => active;
        public int Score => active.Sum(c => c.Score);

        private void Awake()
        {
            active = RunRecord.SelectedConstraints(setup.Campaign);
            foreach (ConstraintDefinition constraint in active)
            {
                modifiers.Apply(constraint.Effects);
            }
        }
    }
}
