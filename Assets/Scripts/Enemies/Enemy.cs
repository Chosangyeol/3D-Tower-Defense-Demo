using System;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// 경로를 따라 기지까지 이동하는 적.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        private EnemyPath path;
        private int targetIndex;
        private bool finished;

        public EnemyDefinition Definition { get; private set; }

        /// <summary>적이 사라질 때 한 번 호출된다. 두 번째 값은 기지에 도달했는지 여부.</summary>
        public event Action<Enemy, bool> Finished;

        public void Initialize(EnemyDefinition definition, EnemyPath enemyPath)
        {
            Definition = definition;
            path = enemyPath;
            targetIndex = 1;
            transform.position = path.GetPoint(0);
            FaceTarget();
        }

        private void Update()
        {
            if (path == null || finished) return;

            float remaining = Definition.MoveSpeed * Time.deltaTime;
            while (remaining > 0f)
            {
                Vector3 target = path.GetPoint(targetIndex);
                float distance = Vector3.Distance(transform.position, target);
                if (distance > remaining)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, remaining);
                    return;
                }

                // 꺾이는 지점에서 남은 이동량을 다음 구간에 이어서 쓴다
                transform.position = target;
                remaining -= distance;
                targetIndex++;
                if (targetIndex >= path.PointCount)
                {
                    Finish(true);
                    Destroy(gameObject);
                    return;
                }
                FaceTarget();
            }
        }

        private void OnDestroy()
        {
            Finish(false);
        }

        private void FaceTarget()
        {
            Vector3 direction = path.GetPoint(targetIndex) - transform.position;
            if (direction.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        private void Finish(bool reachedGoal)
        {
            if (finished) return;

            finished = true;
            Finished?.Invoke(this, reachedGoal);
        }
    }
}
