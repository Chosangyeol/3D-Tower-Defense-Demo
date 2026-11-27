using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// 프로토타입용 임시 안내 UI. 정식 UI가 생기면 삭제한다.
    /// </summary>
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private GameModeController modeController;

        private void OnGUI()
        {
            if (modeController == null) return;

            bool build = modeController.CurrentMode == GameMode.Build;
            string text = build
                ? "[배치 모드]\nWASD/화면 가장자리: 이동 · 휠: 줌 · Q/E: 회전\n좌클릭: 설치 · 우클릭: 제거 · 1~9: 타워 선택\nTab: 전투 모드"
                : "[전투 모드]\nWASD: 이동 · Shift: 달리기 · Space: 점프 · 마우스: 시점\nTab: 배치 모드";

            GUI.Box(new Rect(10, 10, 430, 80), text);

            if (!build)
            {
                // 조준점
                GUI.Label(new Rect(Screen.width * 0.5f - 4f, Screen.height * 0.5f - 11f, 20f, 20f), "+");
            }
        }
    }
}
