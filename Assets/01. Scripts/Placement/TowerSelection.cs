using System.Collections.Generic;
using TowerDefense.Controls;
using TowerDefense.Towers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Placement
{
    /// <summary>
    /// 설치할 타워 종류를 고른다. 선택 칸 입력은 배치 모드에서만 켜진다.
    /// </summary>
    public class TowerSelection : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private TowerDefinition[] towers = new TowerDefinition[0];

        private int selectedIndex;

        /// <summary>설치할 수 있는 타워. 순서가 선택 칸 번호다.</summary>
        public IReadOnlyList<TowerDefinition> Towers => towers;
        public int SelectedIndex => selectedIndex;
        public TowerDefinition Selected => selectedIndex < towers.Length ? towers[selectedIndex] : null;

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

        /// <summary>목록에 있는 번호만 선택된다.</summary>
        public bool Select(int index)
        {
            if (index < 0 || index >= towers.Length) return false;

            selectedIndex = index;
            return true;
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
