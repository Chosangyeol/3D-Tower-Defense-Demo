using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Combat
{
    /// <summary>
    /// 체력. 적, 기지처럼 피해를 받는 모든 오브젝트가 함께 사용한다.
    /// 0이 되었을 때의 처리는 Died 이벤트를 받는 쪽이 맡는다.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private Team team;
        [SerializeField, Min(1f)] private float max = 10f;

        private readonly List<float> damageReductions = new List<float>();

        public Team Team => team;
        public float Max => max;
        public float Current { get; private set; }
        public bool IsAlive => Current > 0f;
        /// <summary>받는 피해에 곱하는 값. 보호막 등이 없으면 1.</summary>
        public float DamageMultiplier
        {
            get
            {
                float multiplier = 1f;
                foreach (float reduction in damageReductions) multiplier *= 1f - reduction;
                return multiplier;
            }
        }

        /// <summary>체력이 0이 되는 순간 한 번 호출된다.</summary>
        public event Action<Health> Died;

        private void Awake()
        {
            Current = max;
        }

        /// <summary>최대 체력을 정하고 가득 채운다. 데이터에서 체력을 가져올 때 사용한다.</summary>
        public void Initialize(float maxHealth)
        {
            max = Mathf.Max(maxHealth, 1f);
            Current = max;
        }

        /// <summary>받는 피해를 비율만큼 줄인다. 여러 개는 곱해서 적용한다.</summary>
        public void AddDamageReduction(float reduction)
        {
            damageReductions.Add(Mathf.Clamp01(reduction));
        }

        public void RemoveDamageReduction(float reduction)
        {
            damageReductions.Remove(Mathf.Clamp01(reduction));
        }

        /// <summary>최대 체력까지 회복한다. 이미 0이 된 대상은 되살리지 않는다.</summary>
        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            Current = Mathf.Min(Current + amount, max);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            Current = Mathf.Max(Current - amount * DamageMultiplier, 0f);
            if (!IsAlive) Died?.Invoke(this);
        }
    }
}
