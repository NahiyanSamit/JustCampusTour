using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;


namespace Player
{
    public class Player : MonoBehaviour
    {
        
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
            
            _moveAction.started += Move;
            _moveAction.canceled += CancelMove;
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _interactAction.Disable();
            _lookAction.Disable();
            
            _moveAction.started -= Move;
            _moveAction.canceled -= CancelMove;
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
            while (true)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    break;
                }
                
                // Perform movement logic here
                gameObject.transform.Translate(direction * Time.deltaTime);
                
                Debug.Log($"Moving in direction: {direction}");
                
                // Simulate some delay
                await UniTask.Delay(100, cancellationToken: cts.Token);
            }
        }
        
    }
    
}
