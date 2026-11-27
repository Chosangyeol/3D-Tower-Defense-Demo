using UnityEngine;

namespace TowerDefense.Magic
{
    /// <summary>
    /// 폭발이 닿은 범위를 잠깐 보여주는 표시. 지름 1짜리 구를 폭발 크기까지 키운 뒤 사라진다. 판정에는 관여하지 않는다.
    /// </summary>
    public class SplashFlash : MonoBehaviour
    {
        private const float StartRatio = 0.3f;

        [SerializeField, Min(0.05f)] private float duration = 0.25f;

        private float diameter;
        private float age;

        public void Show(float radius)
        {
            diameter = radius * 2f;
            Resize();
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= duration)
            {
                Destroy(gameObject);
                return;
            }
            Resize();
        }

        private void Resize()
        {
            transform.localScale = Vector3.one * (diameter * Mathf.Lerp(StartRatio, 1f, age / duration));
        }
    }
}
