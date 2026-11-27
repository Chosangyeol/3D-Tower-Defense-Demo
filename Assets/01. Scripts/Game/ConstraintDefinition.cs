using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Effects;
using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 클리어한 뒤 다시 도전할 때 스스로 거는 제약.
    /// </summary>
    [GameDataCategory("제약", "Constraints", 55)]
    [CreateAssetMenu(menuName = "Tower Defense/Constraint Definition", fileName = "Constraint")]
    public class ConstraintDefinition : GameData
    {
        [SerializeField, TextArea] private string description;
        [Tooltip("난이도 점수. 건 제약의 점수를 합산해 기록한다")]
        [SerializeField, Min(0)] private int score = 1;
        [SerializeField] private GameEffect[] effects = new GameEffect[0];

        public string Description => description;
        public int Score => score;
        public IReadOnlyList<GameEffect> Effects => effects;
    }
}
