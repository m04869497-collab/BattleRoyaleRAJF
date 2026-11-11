using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RAJF
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonPlayerController : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private MobileInputBridge mobileInput;
        [SerializeField, Min(0.1f)] private float baseSpeed = 5.5f;
        [SerializeField, Min(0f)] private float turnSpeed = 720f;

        private float verticalVelocity;
        private float speedBoostTime;
        private float speedMultiplier = 1f;

        public void Configure(CharacterDefinition definition, Transform view, MobileInputBridge input)
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            if (definition != null)
                baseSpeed = definition.MoveSpeed;
            if (view != null)
                cameraTransform = view;
            if (input != null)
                mobileInput = input;
        }

        public void SetTemporarySpeedBoost(float duration, float multiplier)
        {
            speedBoostTime = Mathf.Max(speedBoostTime, duration);
            speedMultiplier = Mathf.Max(1f, multiplier);
        }

        private void Update()
        {
            if (characterController == null)
                return;

            if (speedBoostTime > 0f)
            {
                speedBoostTime -= Time.deltaTime;
                if (speedBoostTime <= 0f)
                    speedMultiplier = 1f;
            }

            Vector2 input = ReadKeyboardMove();
            if (mobileInput != null)
                input += mobileInput.MoveInput;
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward = forward.sqrMagnitude > 0.001f ? forward.normalized : transform.forward;
            right = right.sqrMagnitude > 0.001f ? right.normalized : transform.right;

            Vector3 direction = forward * input.y + right * input.x;
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            if (characterController.isGrounded)
                verticalVelocity = -2f;
            else
                verticalVelocity += Physics.gravity.y * Time.deltaTime;

            Vector3 velocity = direction * (baseSpeed * speedMultiplier);
            velocity.y = verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        private static Vector2 ReadKeyboardMove()
        {
            Vector2 value = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value.x -= 1f;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) value.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) value.y -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) value.x += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) value.x -= 1f;
#endif
            return Vector2.ClampMagnitude(value, 1f);
        }
    }
}
