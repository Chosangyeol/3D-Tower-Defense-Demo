using TowerDefense.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.CameraSystem
{
    /// <summary>
    /// 탑뷰 카메라 리그. 리그(이 오브젝트)는 지면 위의 초점을 이동·회전하고,
    /// 자식 카메라는 거리와 각도만큼 떨어져 초점을 바라본다.
    /// </summary>
    public class TopDownCameraController : MonoBehaviour
    {
        [SerializeField] private GameModeController modeController;
        [SerializeField] private Transform cameraTransform;

        [Header("이동")]
        [SerializeField] private float panSpeed = 18f;
        [SerializeField] private bool edgeScroll = true;
        [SerializeField] private float edgeSize = 12f;
        [SerializeField] private Vector2 panLimit = new Vector2(20f, 20f);

        [Header("줌")]
        [SerializeField] private float distance = 28f;
        [SerializeField] private float minDistance = 12f;
        [SerializeField] private float maxDistance = 45f;
        [SerializeField] private float zoomStep = 3f;
        [SerializeField, Range(30f, 90f)] private float pitch = 60f;

        [Header("회전")]
        [SerializeField] private float rotateSpeed = 90f;

        // 마우스가 화면 안쪽에 한 번 들어온 뒤에만 가장자리 스크롤을 켠다.
        // 플레이 시작 직후의 마우스 위치 (0,0)을 가장자리로 오인하지 않기 위해서다
        private bool edgeArmed;

        // 시작·끊김 직후의 큰 프레임 시간으로 카메라가 튀지 않게 한다
        private const float MaxDeltaTime = 0.05f;

        [Header("입력")]
        [SerializeField] private InputAction moveAction = CreateMoveAction();
        [SerializeField] private InputAction rotateAction = CreateRotateAction();
        [SerializeField] private InputAction zoomAction = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");

        private void OnEnable()
        {
            edgeArmed = false;
            moveAction.Enable();
            rotateAction.Enable();
            zoomAction.Enable();
            ApplyCameraOffset();
        }

        private void OnDisable()
        {
            moveAction.Disable();
            rotateAction.Disable();
            zoomAction.Disable();
        }

        private void Update()
        {
            if (modeController.CurrentMode != GameMode.Build) return;

            float deltaTime = Mathf.Min(Time.unscaledDeltaTime, MaxDeltaTime);
            Rotate(deltaTime);
            Pan(deltaTime);
            Zoom();
            ApplyCameraOffset();
        }

        private void Rotate(float deltaTime)
        {
            float input = rotateAction.ReadValue<float>();
            transform.Rotate(0f, input * rotateSpeed * deltaTime, 0f, Space.World);
        }

        private void Pan(float deltaTime)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            if (edgeScroll) input += ReadEdgeInput();
            input = Vector2.ClampMagnitude(input, 1f);

            // 멀리서 볼수록 빠르게 이동
            float speed = panSpeed * (distance / maxDistance + 0.5f);
            Vector3 position = transform.position + transform.rotation * new Vector3(input.x, 0f, input.y) * (speed * deltaTime);
            position.x = Mathf.Clamp(position.x, -panLimit.x, panLimit.x);
            position.z = Mathf.Clamp(position.z, -panLimit.y, panLimit.y);
            transform.position = position;
        }

        private Vector2 ReadEdgeInput()
        {
            Vector2 mouse = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(-1f, -1f);
            bool outside = mouse.x < 0f || mouse.y < 0f || mouse.x > Screen.width || mouse.y > Screen.height;
            if (outside || !Application.isFocused)
            {
                edgeArmed = false;
                return Vector2.zero;
            }

            Vector2 result = Vector2.zero;
            if (mouse.x <= edgeSize) result.x = -1f;
            else if (mouse.x >= Screen.width - edgeSize) result.x = 1f;
            if (mouse.y <= edgeSize) result.y = -1f;
            else if (mouse.y >= Screen.height - edgeSize) result.y = 1f;

            if (result == Vector2.zero) edgeArmed = true;
            return edgeArmed ? result : Vector2.zero;
        }

        private void Zoom()
        {
            // 휠 값의 크기는 설정에 따라 달라서 방향만 사용한다
            float scroll = zoomAction.ReadValue<float>();
            if (Mathf.Approximately(scroll, 0f)) return;

            distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * zoomStep, minDistance, maxDistance);
        }

        private void ApplyCameraOffset()
        {
            if (cameraTransform == null) return;

            Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
            cameraTransform.localRotation = rotation;
            cameraTransform.localPosition = rotation * new Vector3(0f, 0f, -distance);
        }

        private static InputAction CreateMoveAction()
        {
            var action = new InputAction("Pan", InputActionType.Value);
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            return action;
        }

        private static InputAction CreateRotateAction()
        {
            var action = new InputAction("Rotate", InputActionType.Value);
            action.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/q")
                .With("Positive", "<Keyboard>/e");
            return action;
        }
    }
}
