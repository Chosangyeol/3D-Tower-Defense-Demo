using System;

namespace TowerDefense.Data
{
    /// <summary>
    /// 데이터 관리 창의 데이터 종류 하나를 정의한다.
    /// 이 클래스를 상속한 하위 클래스들은 같은 종류로 묶인다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GameDataCategoryAttribute : Attribute
    {
        /// <param name="label">창에 표시할 종류 이름</param>
        /// <param name="folder">새 에셋을 만들 폴더. Assets/02. Datas 아래의 이름</param>
        /// <param name="order">목록에서의 순서. 작을수록 위</param>
        public GameDataCategoryAttribute(string label, string folder, int order = 0)
        {
            Label = label;
            Folder = folder;
            Order = order;
        }

        public string Label { get; }
        public string Folder { get; }
        public int Order { get; }
    }
}
