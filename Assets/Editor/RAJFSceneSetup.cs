using System;
using System.IO;
using RAJF;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace RAJF.EditorTools
{
    public static class RAJFSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/RAJFPrototype.unity";

        [MenuItem("Tools/RAJF/Create Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Replace prototype scene?",
                    "This will replace " + ScenePath + ".", "Replace", "Cancel"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            try
            {
                Scene scene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene, NewSceneMode.Single);

                EnsureFolder("Assets/Scenes");
                EnsureFolder("Assets/RAJF");
                EnsureFolder("Assets/RAJF/Materials");

                Material groundMaterial = CreateMaterial("Ground", new Color(0.22f, 0.42f, 0.25f));
                Material playerMaterial = CreateMaterial("Player", new Color(0.18f, 0.48f, 0.82f));
                Material botMaterial = CreateMaterial("TrainingBot", new Color(0.78f, 0.24f, 0.19f));
                Material obstacleMaterial = CreateMaterial("Obstacle", new Color(0.38f, 0.38f, 0.42f));
                Material stormMaterial = CreateMaterial("StormBoundary", new Color(0.35f, 0.8f, 1f));

                CreatePrimitive("Training Ground", PrimitiveType.Plane,
                    new Vector3(0f, -0.1f, 0f), new Vector3(8f, 1f, 8f), groundMaterial);
                CreatePrimitive("Cover A", PrimitiveType.Cube,
                    new Vector3(-7f, 1f, 4f), new Vector3(3f, 2f, 2f), obstacleMaterial);
                CreatePrimitive("Cover B", PrimitiveType.Cube,
                    new Vector3(7f, 1f, 5f), new Vector3(2f, 2f, 4f), obstacleMaterial);
                CreatePrimitive("Cover C", PrimitiveType.Cube,
                    new Vector3(1f, 0.75f, 10f), new Vector3(4f, 1.5f, 2f), obstacleMaterial);

                GameObject player = CreateActor("Player", new Vector3(0f, 0f, 0f), playerMaterial);
                HealthShield playerHealth = player.AddComponent<HealthShield>();
                MobileInputBridge mobileInput = player.AddComponent<MobileInputBridge>();
                ThirdPersonPlayerController playerController =
                    player.AddComponent<ThirdPersonPlayerController>();
                WeaponController weapon = player.AddComponent<WeaponController>();
                CharacterAbility ability = player.AddComponent<CharacterAbility>();

                GameObject cameraObject = new GameObject("Third Person Camera", typeof(Camera));
                cameraObject.tag = "MainCamera";
                cameraObject.transform.SetParent(player.transform, false);
                cameraObject.transform.localPosition = new Vector3(0f, 4.6f, -7f);
                cameraObject.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.fieldOfView = 62f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 150f;

                playerController.Configure(null, camera.transform, mobileInput);
                weapon.Configure(camera.transform, mobileInput, null);
                ability.Configure(null, playerHealth, playerController, mobileInput);

                GameObject lightObject = new GameObject("Directional Light", typeof(Light));
                Light light = lightObject.GetComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

                TrainingBot[] bots =
                {
                    CreateBot("Training Bot 1", new Vector3(8f, 0f, 8f), botMaterial),
                    CreateBot("Training Bot 2", new Vector3(-10f, 0f, 7f), botMaterial),
                    CreateBot("Training Bot 3", new Vector3(10f, 0f, -8f), botMaterial),
                    CreateBot("Training Bot 4", new Vector3(-9f, 0f, -10f), botMaterial)
                };

                GameObject sessionObject = new GameObject("RAJF Game Session");
                GameSession session = sessionObject.AddComponent<GameSession>();

                GameObject stormObject = new GameObject("Storm Boundary");
                LineRenderer line = stormObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.loop = false;
                line.widthMultiplier = 0.16f;
                line.positionCount = 65;
                line.sharedMaterial = stormMaterial;
                line.startColor = new Color(0.35f, 0.8f, 1f, 1f);
                line.endColor = new Color(0.35f, 0.8f, 1f, 1f);
                line.numCapVertices = 2;
                StormController storm = stormObject.AddComponent<StormController>();
                storm.Configure(session, playerHealth, player.transform, line, Vector3.zero,
                    32f, 6f, 20f, 90f, 6f);

                CreateEventSystem();
                Canvas canvas = CreateCanvas();
                HUDController hud = canvas.GetComponent<HUDController>();
                hud.Configure(session, playerHealth, mobileInput, ability);

                for (int i = 0; i < bots.Length; i++)
                {
                    if (bots[i] != null)
                        bots[i].Initialize(session, playerHealth, player.transform);
                }

                session.Configure(playerController, playerHealth, ability, weapon, mobileInput,
                    storm, bots, hud);

                if (SceneManager.GetActiveScene() != scene)
                    SceneManager.SetActiveScene(scene);

                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Unity could not save " + ScenePath + ".");

                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(ScenePath, true)
                };
                AssetDatabase.Refresh();
                Selection.activeGameObject = sessionObject;
                Debug.Log("RAJF prototype scene created at " + ScenePath);
                EditorUtility.DisplayDialog("RAJF scene created",
                    "Open " + ScenePath + " and press Play. The scene is also enabled for builds.",
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("RAJF scene creation failed",
                    "Unity could not create the prototype scene:\n\n" + exception.Message +
                    "\n\nCheck the Console for details.", "OK");
            }
        }

        private static GameObject CreateActor(string name, Vector3 position, Material material)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.42f;
            controller.center = Vector3.up;
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45f;

            GameObject visual = CreatePrimitive(name + " Visual", PrimitiveType.Capsule,
                Vector3.zero, Vector3.one, material);
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.up;
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                UnityEngine.Object.DestroyImmediate(visualCollider);
            return root;
        }

        private static TrainingBot CreateBot(string name, Vector3 position, Material material)
        {
            GameObject root = CreateActor(name, position, material);
            root.AddComponent<HealthShield>();
            return root.AddComponent<TrainingBot>();
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type,
            Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            if (item == null)
                throw new InvalidOperationException("Unity could not create primitive: " + name);

            item.name = name;
            item.transform.position = position;
            item.transform.localScale = scale;
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;
            return item;
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("RAJF Canvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(HUDController));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = "Assets/RAJF/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader == null)
                    throw new InvalidOperationException("No compatible Unity shader was found.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string folder = path.Substring(separator + 1);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            if (AssetDatabase.CreateFolder(parent, folder).Length == 0)
                throw new InvalidOperationException("Unity could not create folder: " + path);
        }
    }
}
