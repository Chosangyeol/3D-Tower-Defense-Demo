using UnityEngine;

namespace TowerDefense.Player
{
    /// <summary>
    /// 마법에 쓰는 마나. 배치·전투 모드와 관계없이 시간이 지나면 회복한다.
    /// </summary>
    public class PlayerMana : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float max = 100f;
        [SerializeField, Min(0f)] private float regenPerSecond = 12f;

        public float Max => max;
        public float Current { get; private set; }

        private void Awake()
        {
            Current = max;
        }

        private void Update()
        {
            Current = Mathf.Min(Current + regenPerSecond * Time.deltaTime, max);
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
