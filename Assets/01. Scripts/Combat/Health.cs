using System;
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

        public Team Team => team;
        public float Max => max;
        public float Current { get; private set; }
        public bool IsAlive => Current > 0f;

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

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            Current = Mathf.Max(Current - amount, 0f);
            if (!IsAlive) Died?.Invoke(this);
        }
    }
}
