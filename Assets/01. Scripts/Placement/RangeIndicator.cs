using UnityEngine;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 타워 사거리 원.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RangeIndicator : MonoBehaviour
    {
        [SerializeField, Min(8)] private int segments = 64;
        [SerializeField] private float heightOffset = 0.05f;
        [SerializeField] private float lineWidth = 0.12f;

        private LineRenderer line;

        private LineRenderer Line
        {
            get
            {
                if (line == null)
                {
                    line = GetComponent<LineRenderer>();
                    line.useWorldSpace = true;
                    line.loop = true;
                    line.widthMultiplier = lineWidth;
                }
                return line;
            }
        }

        public void Show(Vector3 center, float radius)
        {
            Line.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                var offset = new Vector3(Mathf.Cos(angle) * radius, heightOffset, Mathf.Sin(angle) * radius);
                Line.SetPosition(i, center + offset);
            }
            Line.enabled = true;
        }

        public void Hide()
        {
            Line.enabled = false;
        }
    }
}
