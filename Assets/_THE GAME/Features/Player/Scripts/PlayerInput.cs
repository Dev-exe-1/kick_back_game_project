using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Features.Player.Scripts
{
    using Camera = UnityEngine.Camera;
    public class PlayerInput : MonoBehaviour
    {

        private Camera _mainCamera;
        private PlayerControls _controls;
        private bool _startedOnUI = false;

        private void Awake()
        {
            _mainCamera = Camera.main;

            _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            _controls.Enable();
            _controls.Player.Fire.started += OnFireStarted;
            _controls.Player.Fire.canceled += OnFireCanceled;
        }

        private void OnDisable()
        {
            _controls.Player.Fire.started -= OnFireStarted;
            _controls.Player.Fire.canceled -= OnFireCanceled;
            _controls.Disable();
        }

        private void OnDestroy()
        {
            _controls.Dispose();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            if (_controls.Player.Fire.IsPressed() && !_startedOnUI)
            {
                HandleAiming();
            }
        }

        private void OnFireStarted(InputAction.CallbackContext context)
        {
            _startedOnUI = IsPointerOverUI();
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (Core.ObjectPoolManager.Instance == null) return;
            
            if (_startedOnUI) return;

            // Force immediate blocking read to guarantee fresh aim data before firing.
            HandleAiming();

            PlayerEvents.RaiseTryShoot();
        }

        private void HandleAiming()
        {
            if (_mainCamera == null) return;

            Vector2 pointerPos = _controls.Player.PointerPosition.ReadValue<Vector2>();

            // Ignore (0,0) often sent when a touch ends on mobile.
            if (pointerPos == Vector2.zero) return;

            Vector3 screenPos = new Vector3(pointerPos.x, pointerPos.y, -_mainCamera.transform.position.z);
            Vector3 worldPointerPosition = _mainCamera.ScreenToWorldPoint(screenPos);

            Vector2 aimDirection = (worldPointerPosition - transform.position).normalized;

            PlayerEvents.RaiseAim(aimDirection);
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null || Pointer.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Pointer.current.position.ReadValue();
            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }
    }
}
