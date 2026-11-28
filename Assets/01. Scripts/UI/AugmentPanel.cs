using System.Collections.Generic;
using TowerDefense.Augments;
using TowerDefense.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.UI
{
    /// <summary>
    /// 프로토타입용 임시 증강 선택 화면. 후보를 카드로 보여주고, 클릭하면 고르거나 다시 뽑는다.
    /// 게임이 멈춘 동안에도 동작하도록 시간은 unscaled를 쓴다. 정식 UI가 생기면 교체한다.
    /// </summary>
    public class AugmentPanel : MonoBehaviour
    {
        private const float CardWidth = 240f;
        private const float CardHeight = 220f;
        private const float Gap = 16f;
        private const float ButtonWidth = 220f;
        private const int Nothing = -1;
        private const int RerollButton = -2;
        // 전투 중에 연타하던 클릭으로 바로 골라지지 않게, 나타난 직후에는 받지 않는다
        private const float InputDelay = 0.5f;

        [SerializeField] private GameInput input;
        [SerializeField] private AugmentSystem augments;

        private float shownAt;
        private int pressed = Nothing;
        private GUIStyle title;
        private GUIStyle body;

        /// <summary>지금 클릭을 받는지 여부.</summary>
        public bool IsReady => augments.IsOffering && Time.unscaledTime - shownAt >= InputDelay;

        /// <summary>후보 카드 영역. GUI 좌표(왼쪽 위 기준).</summary>
        public Rect GetCardRect(int index)
        {
            int count = augments.Choices.Count;
            float total = CardWidth * count + Gap * (count - 1);
            float x = (Screen.width - total) * 0.5f + (CardWidth + Gap) * index;
            return new Rect(x, (Screen.height - CardHeight) * 0.5f, CardWidth, CardHeight);
        }

        /// <summary>다시 뽑기 버튼 영역. GUI 좌표.</summary>
        public Rect GetRerollRect()
        {
            return new Rect((Screen.width - ButtonWidth) * 0.5f, (Screen.height + CardHeight) * 0.5f + Gap, ButtonWidth, 32f);
        }

        private void OnEnable()
        {
            input.Click.performed += OnPress;
            input.Click.canceled += OnRelease;
            augments.OfferShown += OnOfferShown;
        }

        private void OnDisable()
        {
            input.Click.performed -= OnPress;
            input.Click.canceled -= OnRelease;
            augments.OfferShown -= OnOfferShown;
        }

        private void OnOfferShown()
        {
            shownAt = Time.unscaledTime;
            pressed = Nothing;
        }

        private void OnPress(InputAction.CallbackContext context)
        {
            pressed = IsReady ? FindPointed() : Nothing;
        }

        // 누른 순간에 고르면 눌린 클릭이 게임 조작으로 이어지므로, 같은 곳에서 손을 뗄 때 고른다
        private void OnRelease(InputAction.CallbackContext context)
        {
            int target = pressed;
            pressed = Nothing;
            if (target == Nothing || !augments.IsOffering || FindPointed() != target) return;

            if (target == RerollButton) augments.Reroll();
            else augments.Choose(target);
        }

        private int FindPointed()
        {
            Vector2 screen = input.Point.ReadValue<Vector2>();
            var pointer = new Vector2(screen.x, Screen.height - screen.y);

            IReadOnlyList<AugmentDefinition> choices = augments.Choices;
            for (int i = 0; i < choices.Count; i++)
            {
                if (GetCardRect(i).Contains(pointer)) return i;
            }
            return GetRerollRect().Contains(pointer) ? RerollButton : Nothing;
        }

        private void OnGUI()
        {
            if (augments == null || !augments.IsOffering) return;

            // 다른 임시 화면보다 위에 그린다
            GUI.depth = -100;
            EnsureStyles();

            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;

            Rect first = GetCardRect(0);
            GUI.Label(new Rect(0f, first.y - 44f, Screen.width, 28f),
                $"증강 선택 · {augments.OfferTier.ToLabel()} 등급", title);

            bool enabled = GUI.enabled;
            GUI.enabled = IsReady;
            for (int i = 0; i < augments.Choices.Count; i++)
            {
                AugmentDefinition augment = augments.Choices[i];
                Rect card = GetCardRect(i);
                GUI.Box(card, GUIContent.none);
                GUI.Box(card, GUIContent.none);

                GUI.Label(new Rect(card.x + 8f, card.y + 12f, card.width - 16f, 28f), augment.DisplayName, title);
                GUI.Label(new Rect(card.x + 12f, card.y + 52f, card.width - 24f, 20f),
                    $"보유 {augments.GetStacks(augment)}/{augment.MaxStacks}", body);
                GUI.Label(new Rect(card.x + 12f, card.y + 72f, card.width - 24f, 20f), DescribeScope(augment), body);
                GUI.Label(new Rect(card.x + 12f, card.y + 102f, card.width - 24f, card.height - 148f), augment.Description, body);
                GUI.Box(new Rect(card.x + 40f, card.yMax - 40f, card.width - 80f, 28f), "선택", GUI.skin.button);
            }

            GUI.enabled = IsReady && augments.CanReroll;
            GUI.Box(GetRerollRect(), $"다시 뽑기 (남은 횟수 {augments.RerollsLeft})", GUI.skin.button);
            GUI.enabled = enabled;
        }

        // 타워에 관한 증강만 어느 타워에 적용되는지 보여준다
        private static string DescribeScope(AugmentDefinition augment)
        {
            if (!augment.AffectsTowers) return "";

            return augment.Target != null ? $"[{augment.Target.DisplayName} 전용]" : "[모든 타워]";
        }

        private void EnsureStyles()
        {
            if (title != null) return;

            title = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18, fontStyle = FontStyle.Bold };
            body = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, wordWrap = true };
        }
    }
}
