using TowerDefense.Augments;
using TowerDefense.Combat;
using TowerDefense.Economy;
using TowerDefense.Enemies;
using TowerDefense.Game;
using TowerDefense.Magic;
using TowerDefense.Player;
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
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunConstraints constraints;
        [SerializeField] private ConstraintPanel constraintPanel;
        [SerializeField] private PlayerGatherer gatherer;
        [SerializeField] private AugmentSystem augments;
        [SerializeField] private PlayerAltarUser altarUser;

        // 조준한 자원 노드의 안내와 채집 진행도
        private void DrawGathering()
        {
            ResourceNode node = gatherer.Target;
            if (node == null) return;

            string resource = node.Definition.Resource.DisplayName;
            string message = node.IsAvailable
                ? $"E 길게: {resource} 채집 (남은 양 {node.Remaining})"
                : $"{resource} 고갈 · {Mathf.CeilToInt(node.TimeUntilRespawn)}초 뒤 재생";

            DrawAction(message, gatherer.Progress, node.Definition.Resource.Color);
        }

        // 조준한 제단의 비용 안내와 구매 진행도
        private void DrawAltar()
        {
            AugmentAltar altar = altarUser.Target;
            if (altar == null) return;

            string cost = ResourceAmount.Describe(altar.GetCost());
            string message = !altar.HasStock ? "제단: 얻을 수 있는 증강이 없습니다"
                : altar.CanPurchase ? $"E 길게: 증강 구매 ({cost})"
                : $"제단: 자원 부족 ({cost})";
            DrawAction(message, altarUser.Progress, new Color(0.8f, 0.5f, 1f));
        }

        // 조준점 아래의 행동 안내와 진행 막대
        private static void DrawAction(string message, float progress, Color barColor)
        {
            var area = new Rect(Screen.width * 0.5f - 160f, Screen.height * 0.5f + 30f, 320f, 24f);
            GUI.Box(area, message);
            if (progress <= 0f) return;

            Color previous = GUI.color;
            GUI.color = barColor;
            GUI.DrawTexture(new Rect(area.x, area.yMax + 2f, area.width * progress, 6f), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private string DescribeAugments()
        {
            string text = "";
            foreach (AugmentDefinition augment in augments.Acquired)
            {
                text += text.Length > 0 ? ", " : "증강: ";
                int count = augments.GetStacks(augment);
                text += count > 1 ? $"{augment.DisplayName} ×{count}" : augment.DisplayName;
            }
            return text;
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
            // 아래쪽의 제약 화면과 겹치지 않도록 위에 놓는다
            var area = new Rect(Screen.width * 0.5f - 170f, 110f, 340f, 160f);
            bool victory = match.Result == MatchResult.Victory;
            CampaignDefinition campaign = setup.Campaign;
            string message = victory
                ? $"클리어!\n남은 기지 체력 {baseHealth.Current}/{baseHealth.Max} · 난이도 {constraints.Score}"
                : $"패배 · 웨이브 {waveSpawner.StartedWaves}\n기지가 파괴되었습니다";
            message += $"\n최고 웨이브 {RunRecord.BestWave(campaign)} · 클리어 {RunRecord.ClearCount(campaign)}회"
                       + $" · 최고 난이도 {RunRecord.BestScore(campaign)}";
            if (victory && RunRecord.ClearCount(campaign) == 1) message += "\n제약이 해금되었습니다";
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

        // 살아 있는 보스의 이름과 체력 막대를 화면 위 가운데에 보여준다
        private void DrawBosses()
        {
            float width = Mathf.Min(520f, Screen.width * 0.4f);
            float y = 10f;
            foreach (Enemy enemy in Enemy.Active)
            {
                if (!enemy.IsAlive || !enemy.Definition.IsBoss) continue;

                var area = new Rect((Screen.width - width) * 0.5f, y, width, 40f);
                GUI.Box(area, GUIContent.none);
                string state = enemy.Health.DamageMultiplier < 1f ? "  [보호막]" : "";
                if (enemy.IsSlowed) state += "  [감속]";
                if (enemy.Health.DamageBonus > 0f) state += "  [취약]";
                if (enemy.IsAttacking) state += "  [기지 공격 중]";
                GUI.Label(new Rect(area.x + 8f, area.y + 2f, area.width, 20f),
                    $"{enemy.Definition.DisplayName}  {Mathf.CeilToInt(enemy.Health.Current)}/{enemy.Health.Max}{state}");

                Color previous = GUI.color;
                GUI.color = new Color(0.85f, 0.2f, 0.25f);
                float fill = enemy.Health.Current / enemy.Health.Max;
                GUI.DrawTexture(new Rect(area.x + 4f, area.y + 24f, (area.width - 8f) * fill, 12f), Texture2D.whiteTexture);
                GUI.color = previous;
                y += 46f;
            }
        }

        private void OnGUI()
        {
            if (modeController == null) return;

            bool build = modeController.CurrentMode == GameMode.Build;
            string text = build
                ? "[배치 모드]\nWASD/화면 가장자리: 이동 · 휠: 줌 · Q/E: 회전\n하단 버튼·1~9: 설치 · Shift+클릭: 연속 설치 · 우클릭/ESC: 취소\n타워 클릭: 선택 · U: 연구 · F: 2배속 · Tab: 전투 모드"
                : "[전투 모드]\nWASD: 이동 · Shift: 달리기 · Space: 점프 · 마우스: 시점\n좌클릭: 마법 · E: 채집 · Tab: 배치 모드";

            if (waveSpawner != null) text += "\n" + DescribeWave();
            if (constraints.Active.Count > 0) text += $"\n제약 {constraints.Active.Count}개 · 난이도 {constraints.Score}";
            if (waveSpawner.StartedWaves == 0 && constraintPanel.CanOpen) text += "\nC: 제약 선택";
            if (wallet != null) text += "\n" + DescribeResources();
            if (augments.Acquired.Count > 0) text += "\n" + DescribeAugments();

            // 줄 수에 맞춰 상자 높이를 정한다
            float height = GUI.skin.box.CalcHeight(new GUIContent(text), 460f);
            GUI.Box(new Rect(10, 10, 460f, height + 4f), text);

            DrawBosses();

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
                if (altarUser != null) DrawAltar();
            }
        }
    }
}
