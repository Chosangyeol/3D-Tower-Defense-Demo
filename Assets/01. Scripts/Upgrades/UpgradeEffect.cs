using TowerDefense.Combat;
using TowerDefense.Data;

namespace TowerDefense.Upgrades
{
    /// <summary>
    /// 업그레이드의 효과 하나. 특수 기능은 이 클래스를 상속해서 추가하며,
    /// 데이터 관리 창의 "업그레이드 효과"에서 만들 수 있게 된다.
    /// </summary>
    [GameDataCategory("업그레이드 효과", "UpgradeEffects", 70)]
    public abstract class UpgradeEffect : GameData
    {
        public abstract void Apply(AttackSpec spec);
    }
}
