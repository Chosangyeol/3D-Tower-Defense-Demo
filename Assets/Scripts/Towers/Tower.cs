using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 설치된 타워. 타겟팅·공격은 적을 추가할 때 구현한다.
    /// </summary>
    public class Tower : MonoBehaviour
    {
        public TowerDefinition Definition { get; private set; }
        public Vector2Int Cell { get; private set; }

        public void Initialize(TowerDefinition definition, Vector2Int cell)
        {
            Definition = definition;
            Cell = cell;
        }
    }
}
