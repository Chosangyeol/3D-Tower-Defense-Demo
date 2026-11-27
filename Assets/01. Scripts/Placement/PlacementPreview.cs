using TowerDefense.Towers;
using UnityEngine;
using UnityEngine.Rendering;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 배치할 때 보이는 것: 설치 전 고스트(가능·불가 색상)와 사거리 원.
    /// </summary>
    public class PlacementPreview : MonoBehaviour
    {
        [SerializeField] private RangeIndicator rangeIndicator;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private Tower ghost;
        private TowerDefinition ghostDefinition;
        private Renderer[] ghostRenderers;

        /// <summary>빈 칸에 고스트와 사거리를 보여준다.</summary>
        public void ShowGhost(TowerDefinition definition, Vector3 position, bool canPlace)
        {
            EnsureGhost(definition);
            ghost.transform.position = position;
            ghost.gameObject.SetActive(true);
            SetGhostMaterial(canPlace ? validMaterial : invalidMaterial);
            rangeIndicator.Show(position, definition.Range);
        }

        /// <summary>설치된 타워의 사거리만 보여준다.</summary>
        public void ShowRange(Vector3 position, float range)
        {
            HideGhost();
            rangeIndicator.Show(position, range);
        }

        public void Hide()
        {
            HideGhost();
            if (rangeIndicator != null) rangeIndicator.Hide();
        }

        private void HideGhost()
        {
            if (ghost != null) ghost.gameObject.SetActive(false);
        }

        private void EnsureGhost(TowerDefinition definition)
        {
            if (ghost != null && ghostDefinition == definition) return;

            if (ghost != null) Destroy(ghost.gameObject);

            ghostDefinition = definition;
            ghost = Instantiate(definition.Prefab, transform);
            ghost.name = "Ghost";
            ghost.enabled = false;
            foreach (Collider ghostCollider in ghost.GetComponentsInChildren<Collider>())
            {
                ghostCollider.enabled = false;
            }
            ghostRenderers = ghost.GetComponentsInChildren<Renderer>();
            foreach (Renderer ghostRenderer in ghostRenderers)
            {
                ghostRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private void SetGhostMaterial(Material material)
        {
            if (material == null) return;

            foreach (Renderer ghostRenderer in ghostRenderers)
            {
                ghostRenderer.sharedMaterial = material;
            }
        }
    }
}
