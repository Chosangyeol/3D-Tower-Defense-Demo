using System.Collections.Generic;
using TowerDefense.Controls;
using TowerDefense.Towers;
using TowerDefense.Upgrades;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 설치할 타워 종류를 고른다. 해금한 타워만 고를 수 있으며, 선택 칸 입력은 배치 모드에서만 켜진다.
    /// </summary>
    public class TowerSelection : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private TowerDefinition[] towers = new TowerDefinition[0];
        [SerializeField] private ResearchState research;

        private int selectedIndex;

        /// <summary>설치할 수 있는 타워. 순서가 선택 칸 번호다.</summary>
        public IReadOnlyList<TowerDefinition> Towers => towers;
        public int SelectedIndex => selectedIndex;
        /// <summary>고른 타워. 설치할 수 있는 타워가 없으면 null.</summary>
        public TowerDefinition Selected => IsSelectable(selectedIndex) ? towers[selectedIndex] : null;

        private void Start()
        {
            // 첫 칸의 타워가 잠겨 있으면 열려 있는 첫 타워를 고른다
            for (int i = 0; Selected == null && i < towers.Length; i++)
            {
                Select(i);
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < GameInput.SlotCount; i++)
            {
                input.Slot(i).performed += OnSlot;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < GameInput.SlotCount; i++)
            {
                input.Slot(i).performed -= OnSlot;
            }
        }

        /// <summary>목록에 있고 해금한 타워만 선택된다.</summary>
        public bool Select(int index)
        {
            if (!IsSelectable(index)) return false;

            selectedIndex = index;
            return true;
        }

        private bool IsSelectable(int index)
        {
            return index >= 0 && index < towers.Length && research.IsUnlocked(towers[index]);
        }

        private void OnSlot(InputAction.CallbackContext context)
        {
            for (int i = 0; i < GameInput.SlotCount; i++)
            {
                if (input.Slot(i) == context.action) Select(i);
            }
        }
    }
}
