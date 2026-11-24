using UnityEngine;
using UnityEngine.InputSystem;

namespace SalvageSprint
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerDriver : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private float speed = 4.6f;
        private CharacterController controller;
        public void Configure(MatchController owner) { match = owner; }
        private void Awake() { controller = GetComponent<CharacterController>(); }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) { match.Restart(); return; }
            if (!match.IsRunning || !Application.isFocused) return;
            Vector2 input = Vector2.zero;
            if (keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            }
            if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > input.sqrMagnitude)
                input = Gamepad.current.leftStick.ReadValue();
            input = Vector2.ClampMagnitude(input, 1);
            Vector3 velocity = new Vector3(input.x, 0, input.y) * speed;
            controller.Move((velocity + Vector3.down * 2) * Time.deltaTime);
            if (velocity.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(velocity);
        }
    }
}
