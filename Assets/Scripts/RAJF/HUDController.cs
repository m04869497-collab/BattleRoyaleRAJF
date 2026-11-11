using UnityEngine;
using UnityEngine.UI;

namespace RAJF
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameSession session;
        [SerializeField] private HealthShield playerHealth;
        [SerializeField] private MobileInputBridge mobileInput;
        [SerializeField] private CharacterAbility ability;

        private GameObject menuPanel;
        private GameObject gameplayRoot;
        private GameObject endPanel;
        private GameObject mobileControls;
        private Text selectedText;
        private Text healthText;
        private Text shieldText;
        private Text characterText;
        private Text stormText;
        private Text scoreText;
        private Text abilityText;
        private bool built;

        public void Configure(GameSession owner, HealthShield health,
            MobileInputBridge input, CharacterAbility characterAbility)
        {
            session = owner;
            playerHealth = health;
            mobileInput = input;
            ability = characterAbility;
        }

        public void Initialize()
        {
            if (built)
                return;
            if (canvas == null)
                canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("RAJF: HUDController requires a Canvas.");
                return;
            }

            BuildInterface();
            built = true;
            RefreshSelection();
            Refresh();
        }

        public void ShowMainMenu()
        {
            EnsureBuilt();
            if (!built) return;
            menuPanel.SetActive(true);
            gameplayRoot.SetActive(false);
            endPanel.SetActive(false);
        }

        public void ShowGameplay()
        {
            EnsureBuilt();
            if (!built) return;
            menuPanel.SetActive(false);
            gameplayRoot.SetActive(true);
            endPanel.SetActive(false);
            mobileControls.SetActive(true);
            Refresh();
        }

        public void ShowGameOver(int eliminations)
        {
            EnsureBuilt();
            if (!built) return;
            menuPanel.SetActive(false);
            gameplayRoot.SetActive(false);
            endPanel.SetActive(true);
            Text result = endPanel.transform.Find("Result")?.GetComponent<Text>();
            if (result != null)
                result.text = "Health depleted\nEliminations: " + eliminations;
        }

        public void RefreshSelection()
        {
            if (!built || selectedText == null || session == null)
                return;

            CharacterDefinition selected = session.SelectedCharacter;
            selectedText.text = selected == null
                ? "Choose a character"
                : selected.DisplayName + "\nHP " + Mathf.RoundToInt(selected.MaxHealth) +
                  "  |  Shield " + Mathf.RoundToInt(selected.MaxShield) +
                  "  |  Ability cooldown " + Mathf.RoundToInt(selected.AbilityCooldown) + "s";
        }

        public void Refresh()
        {
            if (!built || session == null)
                return;

            if (playerHealth != null)
            {
                if (healthText != null)
                    healthText.text = "Health: " + Mathf.CeilToInt(playerHealth.CurrentHealth) +
                                      " / " + Mathf.CeilToInt(playerHealth.MaxHealth);
                if (shieldText != null)
                    shieldText.text = "Shield: " + Mathf.CeilToInt(playerHealth.CurrentShield);
            }

            if (characterText != null)
                characterText.text = session.SelectedCharacter != null
                    ? session.SelectedCharacter.DisplayName
                    : "No character";
            if (stormText != null)
                stormText.text = session.StormStatus;
            if (scoreText != null)
                scoreText.text = "Eliminations: " + session.Eliminations;
            if (abilityText != null)
                abilityText.text = ability != null ? ability.StatusText : "Ability unavailable";
        }

        private void EnsureBuilt()
        {
            if (!built)
                Initialize();
        }

        private void BuildInterface()
        {
            menuPanel = CreatePanel("MainMenu", transform, new Color(0f, 0f, 0f, 0.82f));
            Stretch(menuPanel.GetComponent<RectTransform>());
            CreateText(menuPanel.transform, "Title", "BATTLE ROYALE RAJF", 36,
                new Vector2(0.5f, 0.82f), new Vector2(620f, 70f));
            CreateText(menuPanel.transform, "Subtitle",
                "Local prototype  |  Training bots only  |  No online play", 18,
                new Vector2(0.5f, 0.75f), new Vector2(700f, 42f));
            CreateText(menuPanel.transform, "ChooseLabel", "Choose a character", 22,
                new Vector2(0.5f, 0.66f), new Vector2(500f, 40f));

            if (session != null)
            {
                for (int i = 0; i < session.Characters.Count; i++)
                {
                    CharacterDefinition item = session.Characters[i];
                    if (item == null) continue;
                    CharacterId id = item.Id;
                    Button characterButton = CreateButton(menuPanel.transform,
                        "Select_" + id, item.DisplayName, 18,
                        new Vector2(0.5f, 0.57f), new Vector2(260f, 42f),
                        new Vector2(0f, -i * 48f));
                    characterButton.onClick.AddListener(() =>
                    {
                        if (session != null) session.SelectCharacter(id);
                    });
                }
            }

            selectedText = CreateText(menuPanel.transform, "SelectedCharacter",
                "Choose a character", 16, new Vector2(0.5f, 0.27f), new Vector2(620f, 52f));
            Button startButton = CreateButton(menuPanel.transform, "StartButton",
                "Start Local Match", 20, new Vector2(0.5f, 0.17f), new Vector2(260f, 52f));
            startButton.onClick.AddListener(() =>
            {
                if (session != null) session.StartMatch();
            });

            gameplayRoot = CreateRoot("GameplayHUD", transform);
            Stretch(gameplayRoot.GetComponent<RectTransform>());
            healthText = CreateText(gameplayRoot.transform, "Health", "Health", 20,
                new Vector2(0f, 1f), new Vector2(320f, 34f), new Vector2(170f, -28f));
            shieldText = CreateText(gameplayRoot.transform, "Shield", "Shield", 20,
                new Vector2(0f, 1f), new Vector2(320f, 34f), new Vector2(170f, -64f));
            characterText = CreateText(gameplayRoot.transform, "Character", "Character", 20,
                new Vector2(0f, 1f), new Vector2(320f, 34f), new Vector2(170f, -100f));
            stormText = CreateText(gameplayRoot.transform, "Storm", "Storm", 20,
                new Vector2(1f, 1f), new Vector2(300f, 34f), new Vector2(-165f, -28f));
            scoreText = CreateText(gameplayRoot.transform, "Score", "Eliminations: 0", 20,
                new Vector2(1f, 1f), new Vector2(300f, 34f), new Vector2(-165f, -64f));
            abilityText = CreateText(gameplayRoot.transform, "AbilityStatus", "Ability: Ready", 18,
                new Vector2(1f, 1f), new Vector2(300f, 34f), new Vector2(-165f, -100f));

            mobileControls = CreateRoot("TouchControls", gameplayRoot.transform);
            Stretch(mobileControls.GetComponent<RectTransform>());
            BuildTouchControls(mobileControls.transform);

            endPanel = CreatePanel("GameOver", transform, new Color(0f, 0f, 0f, 0.86f));
            Stretch(endPanel.GetComponent<RectTransform>());
            CreateText(endPanel.transform, "EndTitle", "MATCH OVER", 34,
                new Vector2(0.5f, 0.62f), new Vector2(520f, 60f));
            CreateText(endPanel.transform, "Result", "Health depleted", 22,
                new Vector2(0.5f, 0.51f), new Vector2(520f, 70f));
            Button restart = CreateButton(endPanel.transform, "RestartButton",
                "Restart Match", 20, new Vector2(0.5f, 0.38f), new Vector2(250f, 52f));
            restart.onClick.AddListener(() =>
            {
                if (session != null) session.StartMatch();
            });

            menuPanel.SetActive(true);
            gameplayRoot.SetActive(false);
            endPanel.SetActive(false);
        }

        private void BuildTouchControls(Transform parent)
        {
            if (mobileInput == null)
                return;

            CreateDirectionalButton(parent, "MoveUp", "▲", 0, new Vector2(0f, 0f),
                new Vector2(132f, 178f));
            CreateDirectionalButton(parent, "MoveDown", "▼", 1, new Vector2(0f, 0f),
                new Vector2(132f, 62f));
            CreateDirectionalButton(parent, "MoveLeft", "◀", 2, new Vector2(0f, 0f),
                new Vector2(68f, 120f));
            CreateDirectionalButton(parent, "MoveRight", "▶", 3, new Vector2(0f, 0f),
                new Vector2(196f, 120f));

            Button fire = CreateButton(parent, "FireTouchButton", "FIRE", 18,
                new Vector2(1f, 0f), new Vector2(112f, 78f), new Vector2(-132f, 95f));
            mobileInput.BindFireButton(fire);

            Button useAbility = CreateButton(parent, "AbilityTouchButton", "ABILITY", 16,
                new Vector2(1f, 0f), new Vector2(132f, 60f), new Vector2(-142f, 190f));
            useAbility.onClick.AddListener(mobileInput.RequestAbility);
        }

        private void CreateDirectionalButton(Transform parent, string name, string label,
            int direction, Vector2 anchor, Vector2 position)
        {
            Button button = CreateButton(parent, name, label, 22, anchor,
                new Vector2(58f, 54f), position);
            mobileInput.BindMoveButton(button, direction);
        }

        private static GameObject CreateRoot(string name, Transform parent)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            return root;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string value, int size,
            Vector2 anchor, Vector2 dimensions, Vector2? offset = null)
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            Place(rect, anchor, dimensions, offset ?? Vector2.zero);
            Text text = item.GetComponent<Text>();
            text.text = value;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (text.font == null)
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
                Debug.LogError("RAJF: Unity's built-in UI font could not be loaded.");
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, int size,
            Vector2 anchor, Vector2 dimensions, Vector2? offset = null)
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            item.transform.SetParent(parent, false);
            Place(item.GetComponent<RectTransform>(), anchor, dimensions, offset ?? Vector2.zero);
            Image image = item.GetComponent<Image>();
            image.color = new Color(0.12f, 0.22f, 0.32f, 0.92f);
            Button button = item.GetComponent<Button>();
            button.targetGraphic = image;
            CreateText(item.transform, "Label", label, size, new Vector2(0.5f, 0.5f),
                dimensions - new Vector2(12f, 8f));
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 dimensions, Vector2 offset)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = offset;
        }
    }
}
