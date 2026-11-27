using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 지금 플레이하는 스테이지. 경로, 웨이브, 기지는 모두 여기의 데이터를 읽어서 서로 어긋나지 않게 한다.
    /// </summary>
    public class StageController : MonoBehaviour
    {
        [SerializeField] private StageDefinition stage;

        public StageDefinition Stage => stage;
    }
}
