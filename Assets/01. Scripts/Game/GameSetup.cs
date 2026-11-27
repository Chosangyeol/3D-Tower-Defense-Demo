using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 이번 판의 맵과 진행. 경로, 기지, 웨이브는 모두 여기의 데이터를 읽어서 서로 어긋나지 않게 한다.
    /// </summary>
    public class GameSetup : MonoBehaviour
    {
        [SerializeField] private MapDefinition map;
        [SerializeField] private CampaignDefinition campaign;

        public MapDefinition Map => map;
        public CampaignDefinition Campaign => campaign;
    }
}
