using TowerDefense.Combat;
using TowerDefense.Economy;
using TowerDefense.Game;
using TowerDefense.Magic;
using TowerDefense.Placement;
using TowerDefense.Player;
using TowerDefense.Towers;
using TowerDefense.Waves;
using UnityEngine;

namespace TowerDefense.UI
{
    /// <summary>
    /// 프로토타입용 임시 안내 UI. 정식 UI가 생기면 삭제한다.
    /// </summary>
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private GameModeController modeController;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private PlayerMana playerMana;
        [SerializeField] private Health baseHealth;
        [SerializeField] private MatchController match;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private TowerSelection selection;
        [SerializeField] private PlayerGatherer gatherer;

        // 조준한 자원 노드의 안내와 채집 진행도
        private void DrawGathering()
        {
            ResourceNode node = gatherer.Target;
            if (node == null) return;

            string resource = node.Definition.Resource.DisplayName;
            string message = node.IsAvailable
                ? $"E 길게: {resource} 채집 (남은 양 {node.Remaining})"
                : $"{resource} 고갈 · {Mathf.CeilToInt(node.TimeUntilRespawn)}초 뒤 재생";

            var area = new Rect(Screen.width * 0.5f - 140f, Screen.height * 0.5f + 30f, 280f, 24f);
            GUI.Box(area, message);
            if (gatherer.Progress > 0f)
            {
                Color previous = GUI.color;
                GUI.color = node.Definition.Resource.Color;
                GUI.DrawTexture(new Rect(area.x, area.yMax + 2f, area.width * gatherer.Progress, 6f), Texture2D.whiteTexture);
                GUI.color = previous;
            }
        }

        private string DescribeResources()
        {
            string text = "";
            foreach (ResourceDefinition kind in wallet.Kinds)
            {
                if (text.Length > 0) text += " · ";
                text += $"{kind.DisplayName} {wallet.GetAmount(kind)}";
            }
            return text;
        }

        private void DrawResult()
        {
            var area = new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 60f, 300f, 120f);
            bool victory = match.Result == MatchResult.Victory;
            string message = victory
                ? $"승리!\n모든 웨이브를 막았습니다\n남은 기지 체력 {baseHealth.Current}/{baseHealth.Max}"
                : "패배\n기지가 파괴되었습니다";
            GUI.Box(area, message);

            if (GUI.Button(new Rect(area.x + 70f, area.yMax - 42f, 160f, 30f), "다시 시작 (R)"))
            {
                match.Restart();
            }
        }

        private void DrawMana()
        {
            const float width = 260f;
            var area = new Rect((Screen.width - width) * 0.5f, Screen.height - 40f, width, 24f);
            GUI.Box(area, GUIContent.none);

            Color previous = GUI.color;
            GUI.color = new Color(0.25f, 0.55f, 1f);
            float fill = playerMana.Current / playerMana.Max;
            GUI.DrawTexture(new Rect(area.x + 2f, area.y + 2f, (area.width - 4f) * fill, area.height - 4f), Texture2D.whiteTexture);
            GUI.color = previous;

            GUI.Label(new Rect(area.x + 8f, area.y + 2f, area.width, area.height), $"마나 {Mathf.FloorToInt(playerMana.Current)}/{playerMana.Max}");
        }

        private string DescribeWave()
        {
            string progress = $"웨이브 {waveSpawner.StartedWaves}/{waveSpawner.WaveCount}";
            string state = waveSpawner.IsWaveRunning ? $"남은 적 {waveSpawner.AliveCount}"
                : waveSpawner.IsAllCleared ? "모든 웨이브 종료"
                : "준비 시간";
            string health = baseHealth != null ? $" · 기지 체력 {baseHealth.Current}/{baseHealth.Max}" : "";
            string text = $"{progress} · {state} · 처치 {waveSpawner.KilledCount}{health}";

            if (waveSpawner.IsPreparing)
            {
                int seconds = Mathf.CeilToInt(waveSpawner.TimeUntilNextWave);
                text += $"\n다음 웨이브까지 {seconds / 60}:{seconds % 60:00} · Enter: 바로 시작";
            }
            return text;
        }

        private void OnGUI()
        {
            if (modeController == null) return;

            bool build = modeController.CurrentMode == GameMode.Build;
            string text = build
                ? "[배치 모드]\nWASD/화면 가장자리: 이동 · 휠: 줌 · Q/E: 회전\n좌클릭: 설치 · 우클릭: 제거 · 1~9: 타워 선택\nU: 연구 · Tab: 전투 모드"
                : "[전투 모드]\nWASD: 이동 · Shift: 달리기 · Space: 점프 · 마우스: 시점\n좌클릭: 마법 · E: 채집 · Tab: 배치 모드";

            if (waveSpawner != null) text += "\n" + DescribeWave();
            if (wallet != null) text += "\n" + DescribeResources();
            if (build && selection != null && selection.Selected != null)
            {
                TowerDefinition tower = selection.Selected;
                text += $"\n선택: {tower.DisplayName} ({ResourceAmount.Describe(tower.Cost)})";
            }

            // 줄 수에 맞춰 상자 높이를 정한다
            float height = GUI.skin.box.CalcHeight(new GUIContent(text), 460f);
            GUI.Box(new Rect(10, 10, 460f, height + 4f), text);

            if (match != null && match.Result != MatchResult.InProgress)
            {
                DrawResult();
                return;
            }

            if (!build)
            {
                // 조준점
                GUI.Label(new Rect(Screen.width * 0.5f - 4f, Screen.height * 0.5f - 11f, 20f, 20f), "+");
                if (playerMana != null) DrawMana();
                if (gatherer != null) DrawGathering();
            }
        }
    }
}
