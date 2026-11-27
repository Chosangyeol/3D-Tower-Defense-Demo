using TowerDefense.Controls;
using TowerDefense.Game;
using UnityEngine;

namespace TowerDefense.Player
{
    /// <summary>
    /// 1인칭 이동·점프·시점. 전투 모드에서만 입력을 받고, 중력은 항상 적용한다.
    /// 공격은 Magic 폴더의 PlayerSpellCaster가 맡는다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private GameModeController modeController;
        [SerializeField] private Transform head;

        [Header("이동")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("시점")]
        [SerializeField] private float lookSensitivity = 0.1f;
        [SerializeField] private float maxPitch = 85f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            bool active = modeController.CurrentMode == GameMode.Combat;
            if (active) Look();
            Move(active);
        }

        private void Look()
        {
            // 커서가 잠기기 전의 마우스 이동은 무시한다
            if (Cursor.lockState != CursorLockMode.Locked) return;

            // 마우스 delta는 프레임당 이동량이라 deltaTime을 곱하지 않는다
            Vector2 delta = input.Look.ReadValue<Vector2>() * lookSensitivity;
            transform.Rotate(0f, delta.x, 0f);
            pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
            head.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move(bool active)
        {
            Vector2 direction = active ? Vector2.ClampMagnitude(input.Move.ReadValue<Vector2>(), 1f) : Vector2.zero;
            float speed = input.Sprint.IsPressed() ? sprintSpeed : walkSpeed;
            Vector3 velocity = transform.rotation * new Vector3(direction.x, 0f, direction.y) * speed;

            if (controller.isGrounded)
            {
                // 지면에 붙어 있도록 약한 하강 속도를 유지
                verticalVelocity = -2f;
                if (active && input.Jump.WasPressedThisFrame())
                {
                    verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
                }
            }

            verticalVelocity += gravity * Time.deltaTime;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
