using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RAJF
{
    public sealed class CharacterAbility : MonoBehaviour
    {
        [SerializeField] private HealthShield health;
        [SerializeField] private ThirdPersonPlayerController player;
        [SerializeField] private MobileInputBridge mobileInput;

        private CharacterDefinition definition;
        private Renderer[] playerRenderers = new Renderer[0];
        private bool[] rendererWasEnabled = new bool[0];
        private float cooldownRemaining;
        private float cloakRemaining;
        private float effectRemaining;
        private bool cloaked;

        public float CooldownRemaining => Mathf.Max(0f, cooldownRemaining);
        public string StatusText
        {
            get
            {
                string cooldown = cooldownRemaining > 0f
                    ? "Cooldown " + Mathf.CeilToInt(cooldownRemaining) + "s"
                    : "Ready";
                return effectRemaining > 0f
                    ? "Ability: Active " + Mathf.CeilToInt(effectRemaining) + "s | " + cooldown
                    : "Ability: " + cooldown;
            }
        }

        public void Configure(CharacterDefinition character, HealthShield ownerHealth,
            ThirdPersonPlayerController owner, MobileInputBridge input)
        {
            definition = character;
            health = ownerHealth;
            player = owner;
            mobileInput = input;
            playerRenderers = player != null ? player.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
            rendererWasEnabled = new bool[playerRenderers.Length];
        }

        public void ResetAbility()
        {
            SetCloak(false);
            cooldownRemaining = 0f;
            cloakRemaining = 0f;
            effectRemaining = 0f;
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
                cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);

            if (effectRemaining > 0f)
                effectRemaining = Mathf.Max(0f, effectRemaining - Time.deltaTime);

            if (cloakRemaining > 0f)
            {
                cloakRemaining -= Time.deltaTime;
                if (cloakRemaining <= 0f)
                    SetCloak(false);
            }

            bool requested = mobileInput != null && mobileInput.ConsumeAbilityRequest();
            if (requested || ReadAbilityPressed())
                TryActivate();
        }

        public bool TryActivate()
        {
            if (definition == null || health == null || health.IsDead || cooldownRemaining > 0f)
                return false;

            float duration = definition.AbilityDuration;
            float power = definition.AbilityPower;
            switch (definition.Id)
            {
                case CharacterId.Scavenger:
                    if (player == null) return false;
                    player.SetTemporarySpeedBoost(duration, power);
                    effectRemaining = duration;
                    break;
                case CharacterId.Phantom:
                    if (duration <= 0f) return false;
                    SetCloak(true);
                    cloakRemaining = duration;
                    effectRemaining = duration;
                    break;
                case CharacterId.MysticMedic:
                    health.Heal(power);
                    break;
                case CharacterId.HeavyDefender:
                    health.AddTemporaryShield(power, duration);
                    effectRemaining = duration;
                    break;
                default:
                    return false;
            }

            cooldownRemaining = definition.AbilityCooldown;
            return true;
        }

        private void SetCloak(bool enabled)
        {
            if (cloaked == enabled)
                return;

            if (enabled)
            {
                if (rendererWasEnabled.Length != playerRenderers.Length)
                    rendererWasEnabled = new bool[playerRenderers.Length];
                for (int i = 0; i < playerRenderers.Length; i++)
                {
                    Renderer item = playerRenderers[i];
                    rendererWasEnabled[i] = item != null && item.enabled;
                    if (item != null)
                        item.enabled = false;
                }
            }
            else
            {
                for (int i = 0; i < playerRenderers.Length; i++)
                {
                    if (playerRenderers[i] != null)
                        playerRenderers[i].enabled = rendererWasEnabled[i];
                }
            }
            cloaked = enabled;
        }

        private static bool ReadAbilityPressed()
        {
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            pressed |= keyboard != null && keyboard.eKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            pressed |= Input.GetKeyDown(KeyCode.E);
#endif
            return pressed;
        }
    }
}
