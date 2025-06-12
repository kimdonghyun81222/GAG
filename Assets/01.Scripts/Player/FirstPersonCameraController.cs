using _01.Scripts.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace _01.Scripts.Player
{
    public class FirstPersonCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private CinemachineCamera virtualCamera;

        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float verticalLookLimit = 80f;

        private float _xRotation = 0f;
        private InputManager _inputManager;

        private void Start()
        {
            _inputManager = InputManager.Instance;
            if (_inputManager == null) Debug.LogError("InputManager not found!");

            if (playerBody == null) Debug.LogError("PlayerBody not assigned in FirstPersonCameraController!");
            if (virtualCamera == null) Debug.LogError("VirtualCamera not assigned in FirstPersonCameraController!");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (_inputManager == null || playerBody == null || virtualCamera == null) return;

            Vector2 lookInput = _inputManager.LookInput;
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
            
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -verticalLookLimit, verticalLookLimit);
            virtualCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}