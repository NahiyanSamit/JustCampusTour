using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;


namespace Player
{
    public class Player : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private GameObject playerHead;
        [SerializeField] private float mouseSensitivity;

        private float _pitch; // up/down
        private float _yaw;   // left/right
        
        // Player Input component
        private PlayerInput _playerInput;
        
        // Input Actions
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _lookAction;
        
        // Cancellation token source for async operations
        private CancellationTokenSource _cancelMove = new CancellationTokenSource();
        
        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Move"];
            _interactAction = _playerInput.actions["Interact"];
            _lookAction = _playerInput.actions["Look"];
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _interactAction.Enable();
            _lookAction.Enable();
            
            _moveAction.performed += Move;
            _moveAction.canceled += CancelMove;
            _lookAction.started += Look;
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _interactAction.Disable();
            _lookAction.Disable();
            
            _moveAction.performed -= Move;
            _moveAction.canceled -= CancelMove;
            _lookAction.started -= Look;
        }
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        
        private void Move(InputAction.CallbackContext context)
        {
            _cancelMove?.Cancel();
            _cancelMove = new CancellationTokenSource();
            
            Vector2 direction = context.ReadValue<Vector2>();
            MoveAsync(direction, _cancelMove).Forget();
        }
        
        private void CancelMove(InputAction.CallbackContext context)
        {
            _cancelMove?.Cancel();
        }
        
        private async UniTask MoveAsync(Vector2 direction, CancellationTokenSource cts)
        {
            while (!cts.Token.IsCancellationRequested)
            {
                Vector3 localDirection = new Vector3(direction.x, 0, direction.y).normalized;
                Vector3 worldDirection = transform.TransformDirection(localDirection);
                transform.position += worldDirection * (moveSpeed * Time.deltaTime);

                await UniTask.Yield(cts.Token);
            }
        }

        
        private void Look(InputAction.CallbackContext context)
        {
            Vector2 lookDelta = context.ReadValue<Vector2>() * mouseSensitivity;

            _yaw += lookDelta.x;
            _pitch -= lookDelta.y;
            _pitch = Mathf.Clamp(_pitch, -80f, 80f); // clamp to prevent flipping

            playerHead.transform.localRotation = Quaternion.Euler(_pitch, 0, 0);
            gameObject.transform.localRotation = Quaternion.Euler(0, _yaw, 0);
        }
        
    }
    
}
