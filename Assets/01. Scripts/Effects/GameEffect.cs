using TowerDefense.Data;

namespace TowerDefense.Effects
{
    /// <summary>
    /// 연구, 제약, 증강이 함께 쓰는 효과 하나. 새 기능은 이 클래스를 상속해서 추가하며,
    /// 데이터 관리 창의 "효과"에서 에셋을 만든다.
    /// </summary>
    [GameDataCategory("효과", "Effects", 70)]
    public abstract class GameEffect : GameData
    {
        /// <summary>타워의 수치를 바꾸는 효과인지 여부. 화면에 적용 대상을 보여줄 때 쓴다.</summary>
        public virtual bool AffectsTowers => false;

        /// <param name="modifiers">이번 판의 수치 변화를 모으는 곳</param>
        /// <param name="defaultTarget">효과를 준 쪽의 대상 (예: 연구의 대상 타워). 없으면 null</param>
        public abstract void Apply(RunModifiers modifiers, GameData defaultTarget);
    }
}
