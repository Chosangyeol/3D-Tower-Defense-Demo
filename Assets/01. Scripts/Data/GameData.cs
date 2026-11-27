using UnityEngine;

namespace TowerDefense.Data
{
    /// <summary>
    /// 게임 데이터(ScriptableObject)의 공통 부모.
    /// 상속한 클래스에 GameDataCategory를 붙이면 데이터 관리 창에 자동으로 나타난다.
    /// </summary>
    public abstract class GameData : ScriptableObject
    {
        [SerializeField] private string displayName;

        /// <summary>화면에 보여줄 이름. 비어 있으면 에셋 이름을 쓴다.</summary>
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    }
}
