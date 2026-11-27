using System;
using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Economy;
using UnityEngine;

namespace TowerDefense.Effects
{
    /// <summary>
    /// 이번 판의 모든 수치 변화(연구, 제약, 증강)를 모은다. 데이터 에셋은 바꾸지 않고, 값을 읽을 때 적용한다.
    /// 최종값 = (기본값 + 더하기 합) × (1 + 비율 합). 적용 순서에 따라 결과가 달라지지 않는다.
    /// </summary>
    [DefaultExecutionOrder(-160)]
    public class RunModifiers : MonoBehaviour
    {
        private readonly Dictionary<(Stat, GameData), Vector2> changes = new Dictionary<(Stat, GameData), Vector2>();

        /// <summary>효과가 더해질 때 호출된다.</summary>
        public event Action Changed;

        public void Add(Stat stat, float flat, float percent, GameData target = null)
        {
            var key = (stat, target);
            changes.TryGetValue(key, out Vector2 current);
            changes[key] = current + new Vector2(flat, percent);
            Changed?.Invoke();
        }

        public void Apply(IReadOnlyList<GameEffect> effects, GameData defaultTarget = null)
        {
            foreach (GameEffect effect in effects)
            {
                if (effect != null) effect.Apply(this, defaultTarget);
            }
        }

        /// <summary>기본값에 전체 효과와 대상 전용 효과를 적용한 값.</summary>
        public float Get(Stat stat, float baseValue, GameData target = null)
        {
            changes.TryGetValue((stat, null), out Vector2 total);
            if (target != null && changes.TryGetValue((stat, target), out Vector2 own)) total += own;
            return Mathf.Max((baseValue + total.x) * (1f + total.y), Minimum(stat));
        }

        public int GetInt(Stat stat, float baseValue, GameData target = null)
        {
            return Mathf.RoundToInt(Get(stat, baseValue, target));
        }

        /// <summary>자원 목록의 각 수량에 배율과 효과를 적용한다.</summary>
        public List<ResourceAmount> Scale(IReadOnlyList<ResourceAmount> amounts, Stat stat,
            float multiplier = 1f, GameData target = null)
        {
            var result = new List<ResourceAmount>(amounts.Count);
            foreach (ResourceAmount item in amounts)
            {
                // 0개는 효과를 더해도 생기지 않는다
                int amount = item.amount > 0 ? GetInt(stat, item.amount * multiplier, target) : 0;
                result.Add(new ResourceAmount(item.resource, amount));
            }
            return result;
        }

        private static float Minimum(Stat stat)
        {
            switch (stat)
            {
                case Stat.TowerFireInterval:
                case Stat.SpellCastInterval:
                    return 0.05f;
                case Stat.TowerProjectileSpeed:
                case Stat.PlayerMaxMana:
                case Stat.EnemyCount:
                case Stat.EnemyHealth:
                case Stat.PrepSeconds:
                case Stat.BaseHealth:
                    return 1f;
                case Stat.EnemyMoveSpeed:
                case Stat.GatherSeconds:
                    return 0.1f;
                default:
                    return 0f;
            }
        }
    }
}
