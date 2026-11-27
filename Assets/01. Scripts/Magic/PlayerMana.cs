using TowerDefense.Player;
using UnityEngine;

namespace TowerDefense.Magic
{
    /// <summary>
    /// 마법에 쓰는 마나. 배치·전투 모드와 관계없이 시간이 지나면 회복한다.
    /// </summary>
    public class PlayerMana : MonoBehaviour
    {
        [SerializeField] private PlayerStatsDefinition stats;

        public float Max => stats.MaxMana;
        public float Current { get; private set; }

        private void Awake()
        {
            Current = Max;
        }

        private void Update()
        {
            Current = Mathf.Min(Current + stats.ManaRegenPerSecond * Time.deltaTime, Max);
        }

        /// <summary>마나가 충분하면 소모하고 true를 돌려준다.</summary>
        public bool TrySpend(float amount)
        {
            if (amount < 0f || Current < amount) return false;

            Current -= amount;
            return true;
        }
    }
}
