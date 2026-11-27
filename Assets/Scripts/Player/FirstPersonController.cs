using TowerDefense.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Player
{
    /// <summary>
    /// 1인칭 이동·점프·시점. 전투 모드에서만 입력을 받고, 중력은 항상 적용한다.
    /// 공격은 PlayerSpellCaster가 맡는다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
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

        [Header("입력")]
        [SerializeField] private InputAction moveAction = CreateMoveAction();
        [SerializeField] private InputAction lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
        [SerializeField] private InputAction jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        [SerializeField] private InputAction sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            moveAction.Enable();
            lookAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable();
            lookAction.Disable();
            jumpAction.Disable();
            sprintAction.Disable();
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
            Vector2 delta = lookAction.ReadValue<Vector2>() * lookSensitivity;
            transform.Rotate(0f, delta.x, 0f);
            pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
            head.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move(bool active)
        {
            Vector2 input = active ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero;
            float speed = sprintAction.IsPressed() ? sprintSpeed : walkSpeed;
            Vector3 velocity = transform.rotation * new Vector3(input.x, 0f, input.y) * speed;

            if (controller.isGrounded)
            {
                // 지면에 붙어 있도록 약한 하강 속도를 유지
                verticalVelocity = -2f;
                if (active && jumpAction.WasPressedThisFrame())
                {
                    verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
                }
            }

            verticalVelocity += gravity * Time.deltaTime;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private static InputAction CreateMoveAction()
        {
            var action = new InputAction("Move", InputActionType.Value);
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            return action;
        }
    }
}
