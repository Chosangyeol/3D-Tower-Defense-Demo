using TowerDefense.Data;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Augments
{
    /// <summary>
    /// 증강을 얻는 방식 하나. 진행 데이터의 "증강 획득 방식" 목록에 넣으면 켜지고 빼면 꺼진다.
    /// 새 방식은 이 클래스와 AugmentSourceRuntime을 상속해서 만들고, 데이터 관리 창의 "증강 획득 방식"에서 에셋을 만든다.
    /// </summary>
    [GameDataCategory("증강 획득 방식", "AugmentSources", 57)]
    public abstract class AugmentSource : GameData
    {
        [Tooltip("이 방식으로 나오는 증강의 등급")]
        [SerializeField] private AugmentTier tier;

        public AugmentTier Tier => tier;

        public abstract AugmentSourceRuntime CreateRuntime(AugmentSystem system);
    }

    /// <summary>한 판 동안의 획득 방식 상태. 데이터 에셋은 바꾸지 않으므로 횟수 같은 값은 여기에 둔다.</summary>
    public abstract class AugmentSourceRuntime
    {
        protected AugmentSourceRuntime(AugmentSystem system)
        {
            System = system;
        }

        protected AugmentSystem System { get; }

        /// <param name="wave">막아낸 웨이브 번호 (1부터)</param>
        public virtual void OnWaveCleared(int wave) { }

        public virtual void OnEnemyKilled(Enemy enemy) { }
    }
}
