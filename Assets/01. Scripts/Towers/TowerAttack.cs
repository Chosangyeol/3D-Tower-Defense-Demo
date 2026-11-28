using System.Collections.Generic;
using TowerDefense.Data;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    /// <summary>
    /// 타워의 공격 방식. 조정하는 수치는 타워 데이터에 있고, 여기에는 공격하는 방법과 그때 쓰는 모형을 둔다.
    /// 새 방식은 이 클래스를 상속해서 만들고, 데이터 관리 창의 "타워 공격 방식"에서 에셋을 만든다.
    /// </summary>
    [GameDataCategory("타워 공격 방식", "TowerAttacks", 5)]
    public abstract class TowerAttack : GameData
    {
        /// <summary>공격할 때 포탑이 대상을 향해 도는지 여부.</summary>
        public virtual bool AimsTurret => true;

        /// <summary>한 번에 공격할 수 있는 대상 수.</summary>
        public virtual int GetTargetCount(TowerStats stats)
        {
            return 1 + stats.ExtraTargets;
        }

        /// <summary>한 번 공격한다.</summary>
        /// <param name="targets">사거리 안에서 앞선 순서로 고른 대상. 하나 이상이다</param>
        /// <param name="stats">이번 판의 효과를 적용한 타워의 수치</param>
        public abstract void Fire(Tower tower, IReadOnlyList<Enemy> targets, TowerStats stats);
    }
}
