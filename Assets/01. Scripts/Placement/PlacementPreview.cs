using TowerDefense.Structures;
using UnityEngine;
using UnityEngine.Rendering;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 배치할 때 보이는 것: 설치 전 고스트(가능·불가 색상)와 범위 원.
    /// </summary>
    public class PlacementPreview : MonoBehaviour
    {
        [SerializeField] private RangeIndicator rangeIndicator;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private Structure ghost;
        private StructureDefinition ghostDefinition;
        private Renderer[] ghostRenderers;

        /// <summary>빈 칸에 고스트와 범위를 보여준다.</summary>
        public void ShowGhost(StructureDefinition definition, Vector3 position, bool canPlace, float range)
        {
            EnsureGhost(definition);
            ghost.transform.position = position;
            ghost.gameObject.SetActive(true);
            SetGhostMaterial(canPlace ? validMaterial : invalidMaterial);
            ShowIndicator(position, range);
        }

        /// <summary>설치된 것의 범위만 보여준다.</summary>
        public void ShowRange(Vector3 position, float range)
        {
            HideGhost();
            ShowIndicator(position, range);
        }

        public void Hide()
        {
            HideGhost();
            if (rangeIndicator != null) rangeIndicator.Hide();
        }

        private void ShowIndicator(Vector3 position, float range)
        {
            if (range > 0f) rangeIndicator.Show(position, range);
            else rangeIndicator.Hide();
        }

        private void HideGhost()
        {
            if (ghost != null) ghost.gameObject.SetActive(false);
        }

        private void EnsureGhost(StructureDefinition definition)
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
