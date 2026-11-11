using System.Collections.Generic;
using UnityEngine;

namespace RAJF
{
    public sealed class GameSession : MonoBehaviour
    {
        [SerializeField] private ThirdPersonPlayerController playerController;
        [SerializeField] private HealthShield playerHealth;
        [SerializeField] private CharacterAbility playerAbility;
        [SerializeField] private WeaponController playerWeapon;
        [SerializeField] private MobileInputBridge mobileInput;
        [SerializeField] private StormController storm;
        [SerializeField] private HUDController hud;
        [SerializeField] private TrainingBot[] trainingBots = new TrainingBot[0];
        [SerializeField] private List<CharacterDefinition> characters = new List<CharacterDefinition>();

        private CharacterDefinition selectedCharacter;
        private int eliminations;

        public bool IsPlaying { get; private set; }
        public bool IsGameOver { get; private set; }
        public int Eliminations => eliminations;
        public CharacterDefinition SelectedCharacter => selectedCharacter;
        public IReadOnlyList<CharacterDefinition> Characters => characters;
        public string StormStatus => storm != null ? storm.StatusText : "Storm unavailable";

        public void Configure(ThirdPersonPlayerController controller, HealthShield health,
            CharacterAbility ability, WeaponController weapon, MobileInputBridge input,
            StormController stormController, TrainingBot[] bots, HUDController hudController)
        {
            playerController = controller;
            playerHealth = health;
            playerAbility = ability;
            playerWeapon = weapon;
            mobileInput = input;
            storm = stormController;
            trainingBots = bots ?? new TrainingBot[0];
            hud = hudController;
            EnsureCharacters();
        }

        private void Awake()
        {
            EnsureCharacters();
            if (playerHealth != null)
                playerHealth.Died += HandlePlayerDeath;
        }

        private void Start()
        {
            if (playerController != null) playerController.enabled = false;
            if (playerWeapon != null) playerWeapon.enabled = false;
            if (playerAbility != null) playerAbility.enabled = false;
            if (trainingBots != null)
            {
                for (int i = 0; i < trainingBots.Length; i++)
                {
                    if (trainingBots[i] != null)
                        trainingBots[i].gameObject.SetActive(false);
                }
            }

            if (hud != null)
            {
                hud.Configure(this, playerHealth, mobileInput, playerAbility);
                hud.Initialize();
                hud.ShowMainMenu();
            }
            else
            {
                Debug.LogError("RAJF: HUDController is not assigned. Run Tools/RAJF/Create Prototype Scene.");
            }
        }

        private void Update()
        {
            if (IsPlaying && hud != null)
                hud.Refresh();
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.Died -= HandlePlayerDeath;
        }

        public void SelectCharacter(CharacterId id)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null && characters[i].Id == id)
                {
                    selectedCharacter = characters[i];
                    if (hud != null)
                        hud.RefreshSelection();
                    return;
                }
            }
        }

        public void StartMatch()
        {
            EnsureCharacters();
            if (selectedCharacter == null && characters.Count > 0)
                selectedCharacter = characters[0];

            if (selectedCharacter == null || playerController == null || playerHealth == null ||
                playerWeapon == null || playerAbility == null)
            {
                Debug.LogError("RAJF: Match setup is incomplete. Recreate the prototype scene from Tools/RAJF.");
                return;
            }

            eliminations = 0;
            IsGameOver = false;
            IsPlaying = true;
            playerHealth.Initialize(selectedCharacter.MaxHealth, selectedCharacter.MaxShield);
            playerController.Configure(selectedCharacter, null, mobileInput);
            playerWeapon.Configure(null, mobileInput, selectedCharacter);
            playerAbility.Configure(selectedCharacter, playerHealth, playerController, mobileInput);
            playerAbility.ResetAbility();
            playerController.enabled = true;
            playerWeapon.enabled = true;
            playerAbility.enabled = true;

            if (mobileInput != null)
                mobileInput.ResetInput();

            if (trainingBots != null)
            {
                for (int i = 0; i < trainingBots.Length; i++)
                {
                    if (trainingBots[i] != null)
                        trainingBots[i].PrepareForMatch(this, playerHealth, playerController.transform, 60f);
                }
            }

            if (storm != null)
                storm.BeginMatch();
            if (hud != null)
                hud.ShowGameplay();
        }

        public void NotifyBotEliminated(TrainingBot bot)
        {
            if (!IsPlaying || bot == null)
                return;
            eliminations++;
            if (hud != null)
                hud.Refresh();
        }

        private void HandlePlayerDeath(HealthShield ignored)
        {
            if (!IsPlaying)
                return;
            IsPlaying = false;
            IsGameOver = true;
            if (playerController != null) playerController.enabled = false;
            if (playerWeapon != null) playerWeapon.enabled = false;
            if (playerAbility != null) playerAbility.enabled = false;
            if (hud != null)
                hud.ShowGameOver(eliminations);
        }

        private void EnsureCharacters()
        {
            if (characters == null)
                characters = new List<CharacterDefinition>();
            if (characters.Count > 0)
            {
                if (selectedCharacter == null)
                    selectedCharacter = characters[0];
                return;
            }

            characters.Add(new CharacterDefinition(CharacterId.Scavenger, "Scavenger",
                100f, 20f, 5.8f, 18f, 40f, 3f, 12f, 1.5f));
            characters.Add(new CharacterDefinition(CharacterId.Phantom, "Phantom",
                95f, 20f, 5.7f, 17f, 40f, 4f, 16f, 0f));
            characters.Add(new CharacterDefinition(CharacterId.MysticMedic, "Mystic Medic",
                110f, 15f, 5.4f, 16f, 38f, 0f, 18f, 35f));
            characters.Add(new CharacterDefinition(CharacterId.HeavyDefender, "Heavy Defender",
                120f, 35f, 4.8f, 20f, 35f, 6f, 20f, 35f));
            selectedCharacter = characters[0];
        }
    }
}
