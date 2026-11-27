using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// 적의 능력 하나. 새 능력은 이 클래스와 EnemyAbilityRuntime을 상속해서 만들고,
    /// 데이터 관리 창의 "적 능력"에서 에셋을 만든다.
    /// 데이터 에셋은 여러 적이 함께 쓰므로, 타이머 같은 실행 중 상태는 Runtime에 둔다.
    /// </summary>
    [GameDataCategory("적 능력", "EnemyAbilities", 15)]
    public abstract class EnemyAbility : GameData
    {
        [Tooltip("체력이 이 비율 이하가 되면 발동한다. 1이면 처음부터")]
        [SerializeField, Range(0f, 1f)] private float activeBelowHealth = 1f;

        public float ActiveBelowHealth => activeBelowHealth;

        public abstract EnemyAbilityRuntime CreateRuntime(Enemy owner);
    }

    /// <summary>적 한 마리가 가진 능력의 실행 상태.</summary>
    public abstract class EnemyAbilityRuntime
    {
        private readonly EnemyAbility ability;

        protected EnemyAbilityRuntime(EnemyAbility ability, Enemy owner)
        {
            this.ability = ability;
            Owner = owner;
        }

        protected Enemy Owner { get; }
        /// <summary>발동 조건을 한 번이라도 만족했는지 여부. 체력이 회복돼도 유지된다.</summary>
        public bool IsActivated { get; private set; }

        public void Update(float healthRatio, float deltaTime)
        {
            if (!IsActivated)
            {
                if (healthRatio > ability.ActiveBelowHealth) return;

                IsActivated = true;
                OnActivated();
            }
            OnTick(deltaTime);
        }

        /// <summary>발동하는 순간 한 번 호출된다.</summary>
        protected virtual void OnActivated() { }

        /// <summary>발동한 뒤 매 프레임 호출된다.</summary>
        protected virtual void OnTick(float deltaTime) { }
    }
}
