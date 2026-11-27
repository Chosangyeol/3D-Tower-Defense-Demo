using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Effects;
using UnityEngine;

namespace TowerDefense.Augments
{
    /// <summary>
    /// 증강 하나. 고르면 그 판이 끝날 때까지 효과가 적용되며, 최대 중첩 횟수까지 다시 나올 수 있다.
    /// 판을 시작할 때만 읽는 수치(시작 자원, 기지 체력)는 증강으로 바꿔도 적용되지 않는다.
    /// </summary>
    [GameDataCategory("증강", "Augments", 56)]
    [CreateAssetMenu(menuName = "Tower Defense/Augment Definition", fileName = "Augment")]
    public class AugmentDefinition : GameData
    {
        [SerializeField, TextArea] private string description;
        [SerializeField] private AugmentTier tier;
        [Tooltip("한 판에서 고를 수 있는 최대 횟수")]
        [SerializeField, Min(1)] private int maxStacks = 1;
        [SerializeField] private GameEffect[] effects = new GameEffect[0];

        public string Description => description;
        public AugmentTier Tier => tier;
        public int MaxStacks => maxStacks;
        public IReadOnlyList<GameEffect> Effects => effects;
    }
}
