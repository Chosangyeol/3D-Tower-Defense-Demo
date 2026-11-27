using UnityEngine;

namespace TowerDefense.Augments
{
    /// <summary>증강의 등급. 번호는 데이터에 저장되므로 바꾸지 않고 새 번호로 추가한다.</summary>
    public enum AugmentTier
    {
        [InspectorName("일반")] Common = 0,
        [InspectorName("상위")] Superior = 1
    }

    public static class AugmentTierNames
    {
        /// <summary>화면에 보여줄 등급 이름.</summary>
        public static string ToLabel(this AugmentTier tier)
        {
            return tier == AugmentTier.Superior ? "상위" : "일반";
        }
    }
}
