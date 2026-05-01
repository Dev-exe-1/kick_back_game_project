using UnityEngine;
using UnityEngine.InputSystem;
using Core;

namespace Features.Player.Scripts
{
    using Camera = UnityEngine.Camera;
    public class PlayerInput : MonoBehaviour
    {

        private Camera _mainCamera;

        // This requires you to check "Generate C# Class" on your Input Action Asset,
        // and name the asset "PlayerControls" with an Action Map named "Player".
        private PlayerControls _controls;

        private void Awake()
        {
            _mainCamera = Camera.main;

            _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            _controls.Enable();
            // ATOMIC FIRE BINDING: Hook into 'performed' to guarantee immediate execution.
            // This happens before Update(), ensuring we don't shoot with a stale frame's aim.
            _controls.Player.Fire.performed += OnFirePerformed;
        }

        private void OnDisable()
        {
            _controls.Player.Fire.performed -= OnFirePerformed;
            _controls.Disable();
        }

        private void OnDestroy()
        {
            _controls.Dispose();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            // Continuously update aiming visually during normal frames
            HandleAiming();
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (Core.ObjectPoolManager.Instance == null) return;

            // 1. ATOMIC UPDATE: Force an immediate, blocking read and calculation of the pointer 
            // position before firing to guarantee no single-frame delays or stale aim data.
            HandleAiming();

            // 2. DECOUPLED COMMUNICATION: Now that the aim is 100% updated, attempt the shot.
            PlayerEvents.RaiseTryShoot();
        }

        private void HandleAiming()
        {
            if (_mainCamera == null) return;

            // 3. INPUT LOGIC: Read the absolute latest value directly from the Input System
            Vector2 pointerPos = _controls.Player.PointerPosition.ReadValue<Vector2>();

            // 4. ZERO-VALUE PROTECTION: Ignore (0,0) as it is often sent when a touch ends on mobile
            if (pointerPos == Vector2.zero) return;

            // Correct Z-plane logic for calculating the world position
            Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, -_mainCamera.transform.position.z);
            Vector3 worldPointerPosition = _mainCamera.ScreenToWorldPoint(screenPos);

            Vector2 aimDirection = (worldPointerPosition - transform.position).normalized;

            // Broadcast the updated aim direction decoupled through the event hub
            PlayerEvents.RaiseAim(aimDirection);
        }
    }
}
