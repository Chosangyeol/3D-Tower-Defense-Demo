using System;
using TowerDefense.Controls;
using UnityEngine;

namespace TowerDefense.Game
{
    /// <summary>
    /// 선택 화면처럼 게임을 잠시 멈추는 곳. 시간을 멈추고, 포인터와 클릭을 뺀 모든 조작을 막는다.
    /// 시점 모드는 바꾸지 않으므로 다시 진행하면 멈추기 전 상태로 돌아간다.
    /// </summary>
    public class GamePause : MonoBehaviour
    {
        [SerializeField] private GameInput input;

        private float previousTimeScale = 1f;

        public bool IsPaused { get; private set; }

        /// <summary>멈추거나 다시 진행할 때 호출된다.</summary>
        public event Action<bool> Changed;

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused) return;

            IsPaused = paused;
            if (paused) previousTimeScale = Time.timeScale;
            Time.timeScale = paused ? 0f : previousTimeScale;
            input.SetMenuOnly(paused);
            Changed?.Invoke(paused);
        }

        private void OnDestroy()
        {
            // 멈춘 상태가 다음 씬이나 다음 플레이에 남지 않게 한다
            if (IsPaused) Time.timeScale = 1f;
        }
    }
}
