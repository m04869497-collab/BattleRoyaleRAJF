using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RAJF
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform aimCamera;
        [SerializeField] private MobileInputBridge mobileInput;
        [SerializeField, Min(1f)] private float damage = 18f;
        [SerializeField, Min(1f)] private float range = 40f;
        [SerializeField, Min(0.05f)] private float secondsBetweenShots = 0.25f;

        private float nextShotTime;

        public void Configure(Transform view, MobileInputBridge input, CharacterDefinition definition)
        {
            if (view != null)
                aimCamera = view;
            if (input != null)
                mobileInput = input;
            if (definition != null)
            {
                damage = definition.WeaponDamage;
                range = definition.WeaponRange;
            }
        }

        private void Update()
        {
            if (ReadFireHeld())
                Fire();
        }

        private bool ReadFireHeld()
        {
            bool held = mobileInput != null && mobileInput.FireHeld;
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            held |= mouse != null && mouse.leftButton.isPressed;
            held |= keyboard != null && keyboard.spaceKey.isPressed;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            held |= Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
#endif
            return held;
        }

        private void Fire()
        {
            if (Time.time < nextShotTime)
                return;
            nextShotTime = Time.time + secondsBetweenShots;

            Camera view = aimCamera != null ? aimCamera.GetComponent<Camera>() : null;
            if (view == null)
                view = Camera.main;

            Ray ray = view != null
                ? view.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f))
                : new Ray(transform.position + Vector3.up, transform.forward);

            RaycastHit[] hits = Physics.RaycastAll(ray, range, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            bool hasHit = false;
            RaycastHit closestHit = default;
            float closestDistance = range;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i].collider;
                if (collider == null)
                    continue;
                Transform hitTransform = collider.transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                    continue;
                if (!hasHit || hits[i].distance < closestDistance)
                {
                    hasHit = true;
                    closestHit = hits[i];
                    closestDistance = hits[i].distance;
                }
            }

            if (!hasHit)
                return;
            TrainingBot bot = closestHit.collider.GetComponentInParent<TrainingBot>();
            if (bot != null)
                bot.ApplyDamage(damage);
        }
    }
}
