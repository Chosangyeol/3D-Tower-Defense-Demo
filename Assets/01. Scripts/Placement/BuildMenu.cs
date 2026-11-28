using System.Collections.Generic;
using TowerDefense.Structures;
using UnityEngine;
using UnityEngine.Serialization;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 설치할 수 있는 것의 목록. 하단 설치 막대에 이 순서로 나오고, 순서가 숫자 키 번호다.
    /// </summary>
    public class BuildMenu : MonoBehaviour
    {
        [FormerlySerializedAs("towers")]
        [SerializeField] private StructureDefinition[] entries = new StructureDefinition[0];

        public IReadOnlyList<StructureDefinition> Entries => entries;
    }
}
