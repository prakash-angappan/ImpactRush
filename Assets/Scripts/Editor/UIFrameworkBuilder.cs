using System.IO;
using ImpactRush.Audio;
using ImpactRush.Core.Bootstrap;
using ImpactRush.Core.Managers;
using ImpactRush.Gameplay;
using ImpactRush.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ImpactRush.Editor
{
    /// <summary>
    /// Builds the reusable UIRoot prefab, placeholder audio, and scene wiring for FEATURE-021.
    /// </summary>
    public static class UIFrameworkBuilder
    {
        private const string UiRootPrefabPath = "Assets/Prefabs/UI/UIRoot.prefab";
        private const string AudioLibraryPath = "Assets/ScriptableObjects/AudioLibrary.asset";
        private const string AudioFolderPath = "Assets/Resources/Audio";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Impact Rush/Build UI Framework")]
        public static void BuildUIFramework()
        {
            EnsureFolders();
            var clips = CreateOrLoadPlaceholderClips();
            var library = CreateOrUpdateAudioLibrary(clips);
            var uiRootPrefab = BuildUIRootPrefab(library);
            WireBootstrapScene(uiRootPrefab);
            WireGameplayScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UI Framework build completed.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Path.GetFullPath("Assets/Prefabs/UI"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/Audio"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/Sprites"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/Fonts"));
        }

        private static AudioLibrary CreateOrUpdateAudioLibrary(PlaceholderClips clips)
        {
            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AudioLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, AudioLibraryPath);
            }

            var serialized = new SerializedObject(library);
            serialized.FindProperty("_backgroundMusic").objectReferenceValue = clips.BackgroundMusic;
            serialized.FindProperty("_buttonClick").objectReferenceValue = clips.ButtonClick;
            serialized.FindProperty("_projectileFire").objectReferenceValue = clips.ProjectileFire;
            serialized.FindProperty("_impact").objectReferenceValue = clips.Impact;
            serialized.FindProperty("_victory").objectReferenceValue = clips.Victory;
            serialized.FindProperty("_popupOpen").objectReferenceValue = clips.PopupOpen;
            serialized.FindProperty("_popupClose").objectReferenceValue = clips.PopupClose;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return library;
        }

        private static PlaceholderClips CreateOrLoadPlaceholderClips()
        {
            return new PlaceholderClips
            {
                BackgroundMusic = CreateToneClip("BackgroundMusic", 220f, 2f, 0.08f, loopFriendly: true),
                ButtonClick = CreateToneClip("ButtonClick", 880f, 0.08f, 0.25f),
                ProjectileFire = CreateToneClip("ProjectileFire", 520f, 0.12f, 0.2f),
                Impact = CreateToneClip("Impact", 140f, 0.18f, 0.3f),
                Victory = CreateToneClip("Victory", 660f, 0.45f, 0.22f),
                PopupOpen = CreateToneClip("PopupOpen", 740f, 0.07f, 0.18f),
                PopupClose = CreateToneClip("PopupClose", 420f, 0.07f, 0.16f),
            };
        }

        private static AudioClip CreateToneClip(string name, float frequency, float duration, float volume, bool loopFriendly = false)
        {
            var assetPath = $"{AudioFolderPath}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 44100;
            var sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var data = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = loopFriendly ? 1f : 1f - (t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            AssetDatabase.CreateAsset(clip, assetPath);
            return clip;
        }

        private static GameObject BuildUIRootPrefab(AudioLibrary library)
        {
            var root = new GameObject("UIRoot");
            root.AddComponent<UIRoot>();

            var audioManager = root.AddComponent<AudioManager>();
            var popupManager = root.AddComponent<PopupManager>();
            var transitionManager = root.AddComponent<SceneTransitionManager>();
            var uiManager = root.AddComponent<UIManager>();

            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(root.transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>().ConfigurePortrait();
            canvasGo.AddComponent<GraphicRaycaster>();

            var safeArea = CreateRect("SafeArea", canvasGo.transform);
            StretchFull(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            var mainMenu = BuildMainMenuScreen(safeArea);
            var gameHud = BuildGameHudScreen(safeArea);
            var settingsPopup = BuildSettingsPopup(safeArea);
            var pausePopup = BuildPausePopup(safeArea);
            var exitPopup = BuildExitPopup(safeArea);
            var levelCompletePopup = BuildLevelCompletePopup(safeArea);
            var loadingPopup = BuildLoadingPopup(safeArea);
            var fadeOverlay = BuildFadeOverlay(canvasGo.transform);

            WireManagerReferences(
                uiManager,
                popupManager,
                transitionManager,
                audioManager,
                library,
                mainMenu,
                gameHud,
                settingsPopup,
                pausePopup,
                exitPopup,
                levelCompletePopup,
                loadingPopup,
                fadeOverlay);

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(root.transform, false);
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, UiRootPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void WireManagerReferences(
            UIManager uiManager,
            PopupManager popupManager,
            SceneTransitionManager transitionManager,
            AudioManager audioManager,
            AudioLibrary library,
            MainMenuScreenView mainMenu,
            GameHudScreenView gameHud,
            SettingsPopupView settingsPopup,
            PausePopupView pausePopup,
            ExitConfirmationPopupView exitPopup,
            LevelCompletePopupView levelCompletePopup,
            LoadingPopupView loadingPopup,
            FadeOverlayView fadeOverlay)
        {
            SetSerializedReference(uiManager, "_mainMenuScreen", mainMenu);
            SetSerializedReference(uiManager, "_gameHudScreen", gameHud);
            SetSerializedReference(uiManager, "_popupManager", popupManager);

            SetSerializedReference(popupManager, "_settingsPopup", settingsPopup);
            SetSerializedReference(popupManager, "_pausePopup", pausePopup);
            SetSerializedReference(popupManager, "_exitConfirmationPopup", exitPopup);
            SetSerializedReference(popupManager, "_levelCompletePopup", levelCompletePopup);
            SetSerializedReference(popupManager, "_loadingPopup", loadingPopup);

            SetSerializedReference(transitionManager, "_fadeOverlay", fadeOverlay);
            SetSerializedReference(transitionManager, "_popupManager", popupManager);

            SetSerializedReference(audioManager, "_library", library);
        }

        private static MainMenuScreenView BuildMainMenuScreen(RectTransform parent)
        {
            var screen = CreateScreenRoot<MainMenuScreenView>(UIScreenIds.MainMenu, parent);
            var layout = CreateVerticalLayout(screen.transform, TextAnchor.MiddleCenter, 28f);

            CreateLabel("Title", layout, "Impact Rush", 72, FontStyles.Bold);
            CreateSpacer(layout, 48f);
            var play = CreateButton(layout, "PlayButton", "Play");
            var settings = CreateButton(layout, "SettingsButton", "Settings");
            var exit = CreateButton(layout, "ExitButton", "Exit");

            SetSerializedReference(screen, "_titleLabel", screen.transform.Find("Title")?.GetComponent<TextMeshProUGUI>());
            SetSerializedReference(screen, "_playButton", play);
            SetSerializedReference(screen, "_settingsButton", settings);
            SetSerializedReference(screen, "_exitButton", exit);
            screen.gameObject.SetActive(false);
            return screen;
        }

        private static GameHudScreenView BuildGameHudScreen(RectTransform parent)
        {
            var screen = CreateScreenRoot<GameHudScreenView>(UIScreenIds.GameHud, parent);
            StretchFull(screen.transform as RectTransform);

            var topBar = CreateRect("TopBar", screen.transform);
            StretchTop(topBar, 140f);
            var topLayout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            topLayout.padding = new RectOffset(24, 24, 24, 24);
            topLayout.childAlignment = TextAnchor.MiddleCenter;
            topLayout.childControlWidth = true;
            topLayout.childForceExpandWidth = true;

            var pause = CreateButton(topBar, "PauseButton", "Pause", 220f);
            var level = CreateLabel("LevelLabel", topBar, "LEVEL 1", 42, FontStyles.Bold);
            var coins = CreateLabel("CoinsLabel", topBar, "0", 42, FontStyles.Normal);

            var bottomArea = CreateRect("BottomArea", screen.transform);
            StretchBottom(bottomArea, 220f);

            SetSerializedReference(screen, "_pauseButton", pause);
            SetSerializedReference(screen, "_levelLabel", level);
            SetSerializedReference(screen, "_coinsLabel", coins);
            screen.gameObject.SetActive(false);
            return screen;
        }

        private static SettingsPopupView BuildSettingsPopup(RectTransform parent)
        {
            var popup = CreatePopupRoot<SettingsPopupView>(UIPopupIds.Settings, parent, "Settings");
            var body = popup.transform.Find("Panel/Body");
            var musicToggle = CreateToggle(body, "MusicToggle", "Music");
            var musicSlider = CreateSlider(body, "MusicVolumeSlider");
            var sfxToggle = CreateToggle(body, "SfxToggle", "Sound Effects");
            var sfxSlider = CreateSlider(body, "SfxVolumeSlider");
            var close = CreateButton(body, "CloseButton", "Close");

            SetSerializedReference(popup, "_musicToggle", musicToggle);
            SetSerializedReference(popup, "_musicVolumeSlider", musicSlider);
            SetSerializedReference(popup, "_sfxToggle", sfxToggle);
            SetSerializedReference(popup, "_sfxVolumeSlider", sfxSlider);
            SetSerializedReference(popup, "_closeButton", close);
            return popup;
        }

        private static PausePopupView BuildPausePopup(RectTransform parent)
        {
            var popup = CreatePopupRoot<PausePopupView>(UIPopupIds.Pause, parent, "Paused");
            var body = popup.transform.Find("Panel/Body");
            var resume = CreateButton(body, "ResumeButton", "Resume");
            var mainMenu = CreateButton(body, "MainMenuButton", "Main Menu");
            SetSerializedReference(popup, "_resumeButton", resume);
            SetSerializedReference(popup, "_mainMenuButton", mainMenu);
            return popup;
        }

        private static ExitConfirmationPopupView BuildExitPopup(RectTransform parent)
        {
            var popup = CreatePopupRoot<ExitConfirmationPopupView>(UIPopupIds.ExitConfirmation, parent, "Exit");
            var body = popup.transform.Find("Panel/Body");
            var message = CreateLabel("Message", body, "Are you sure you want to quit?", 34, FontStyles.Normal);
            var yes = CreateButton(body, "YesButton", "Yes");
            var no = CreateButton(body, "NoButton", "No");
            SetSerializedReference(popup, "_messageLabel", message);
            SetSerializedReference(popup, "_yesButton", yes);
            SetSerializedReference(popup, "_noButton", no);
            return popup;
        }

        private static LevelCompletePopupView BuildLevelCompletePopup(RectTransform parent)
        {
            var popup = CreatePopupRoot<LevelCompletePopupView>(UIPopupIds.LevelComplete, parent, "LEVEL COMPLETE");
            var body = popup.transform.Find("Panel/Body");
            var title = popup.transform.Find("Panel/Header/Title")?.GetComponent<TextMeshProUGUI>();
            var restart = CreateButton(body, "RestartButton", "Restart");
            var next = CreateButton(body, "NextLevelButton", "Next Level");
            var mainMenu = CreateButton(body, "MainMenuButton", "Main Menu");
            SetSerializedReference(popup, "_titleLabel", title);
            SetSerializedReference(popup, "_restartButton", restart);
            SetSerializedReference(popup, "_nextLevelButton", next);
            SetSerializedReference(popup, "_mainMenuButton", mainMenu);
            return popup;
        }

        private static LoadingPopupView BuildLoadingPopup(RectTransform parent)
        {
            var popup = CreatePopupRoot<LoadingPopupView>(UIPopupIds.Loading, parent, "Loading");
            var body = popup.transform.Find("Panel/Body");
            var message = CreateLabel("Message", body, "Loading...", 36, FontStyles.Normal);
            SetSerializedReference(popup, "_messageLabel", message);
            return popup;
        }

        private static FadeOverlayView BuildFadeOverlay(Transform parent)
        {
            var overlay = CreateRect("FadeOverlay", parent);
            StretchFull(overlay);
            overlay.SetAsLastSibling();
            var image = overlay.gameObject.AddComponent<Image>();
            image.color = Color.black;
            var group = overlay.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            var view = overlay.gameObject.AddComponent<FadeOverlayView>();
            SetSerializedReference(view, "_canvasGroup", group);
            SetSerializedReference(view, "_fadeImage", image);
            return view;
        }

        private static T CreateScreenRoot<T>(string id, RectTransform parent) where T : UIScreenView
        {
            var rect = CreateRect(id, parent);
            StretchFull(rect);
            var screen = rect.gameObject.AddComponent<T>();
            SetSerializedReference(screen, "_screenId", id);
            return screen;
        }

        private static T CreatePopupRoot<T>(string id, RectTransform parent, string title) where T : UIPopupView
        {
            var rect = CreateRect(id, parent);
            StretchFull(rect);
            var dim = CreateImage("Dim", rect, new Color(0f, 0f, 0f, 0.65f));
            StretchFull(dim.rectTransform);

            var panel = CreateRect("Panel", rect);
            panel.sizeDelta = new Vector2(860f, 980f);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            CreateImage("Background", panel, new Color(0.12f, 0.14f, 0.2f, 0.96f));
            StretchFull(panel);

            var header = CreateRect("Header", panel);
            StretchTop(header, 120f);
            CreateLabel("Title", header, title, 44, FontStyles.Bold);

            var body = CreateRect("Body", panel);
            body.anchorMin = new Vector2(0f, 0f);
            body.anchorMax = new Vector2(1f, 1f);
            body.offsetMin = new Vector2(48f, 48f);
            body.offsetMax = new Vector2(-48f, -140f);
            var bodyLayout = body.gameObject.AddComponent<VerticalLayoutGroup>();
            bodyLayout.spacing = 18f;
            bodyLayout.childAlignment = TextAnchor.UpperCenter;
            bodyLayout.childControlHeight = false;
            bodyLayout.childControlWidth = true;
            bodyLayout.childForceExpandWidth = true;

            var popup = rect.gameObject.AddComponent<T>();
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            SetSerializedReference(popup, "_canvasGroup", group);
            SetSerializedReference(popup, "_contentRoot", panel);
            rect.gameObject.SetActive(false);
            return popup;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform as RectTransform;
        }

        private static RectTransform CreateVerticalLayout(Transform parent, TextAnchor alignment, float spacing)
        {
            var rect = CreateRect("Layout", parent);
            StretchFull(rect);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = alignment;
            layout.spacing = spacing;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            return rect;
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, float size, FontStyles style)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(800f, 80f);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, string label, float width = 0f)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(width > 0f ? width : 560f, 96f);
            CreateImage("Background", rect, new Color(0.18f, 0.45f, 0.95f, 1f));
            var button = rect.gameObject.AddComponent<Button>();
            rect.gameObject.AddComponent<UIButtonAnimator>();
            var text = CreateLabel("Label", rect, label, 34, FontStyles.Bold);
            StretchFull(text.rectTransform);
            return button;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(700f, 72f);
            var toggle = rect.gameObject.AddComponent<Toggle>();
            var background = CreateImage("Background", rect, new Color(0.2f, 0.22f, 0.28f, 1f));
            background.rectTransform.sizeDelta = new Vector2(72f, 72f);
            background.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            background.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            background.rectTransform.anchoredPosition = new Vector2(36f, 0f);
            var check = CreateImage("Checkmark", background.rectTransform, new Color(0.3f, 0.85f, 0.45f, 1f));
            StretchFull(check.rectTransform);
            var text = CreateLabel("Label", rect, label, 32, FontStyles.Normal);
            text.rectTransform.anchorMin = new Vector2(0f, 0f);
            text.rectTransform.offsetMin = new Vector2(100f, 0f);
            toggle.targetGraphic = background;
            toggle.graphic = check;
            return toggle;
        }

        private static Slider CreateSlider(Transform parent, string name)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(700f, 48f);
            var slider = rect.gameObject.AddComponent<Slider>();
            var background = CreateImage("Background", rect, new Color(0.18f, 0.2f, 0.24f, 1f));
            StretchFull(background.rectTransform);
            var fillArea = CreateRect("Fill Area", rect);
            StretchFull(fillArea);
            fillArea.offsetMin = new Vector2(10f, 10f);
            fillArea.offsetMax = new Vector2(-10f, -10f);
            var fill = CreateImage("Fill", fillArea, new Color(0.25f, 0.65f, 1f, 1f));
            StretchFull(fill.rectTransform);
            var handle = CreateRect("Handle", rect);
            handle.sizeDelta = new Vector2(28f, 28f);
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = Color.white;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            return slider;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            var rect = CreateRect("Spacer", parent);
            rect.sizeDelta = new Vector2(10f, height);
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void StretchBottom(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void WireBootstrapScene(GameObject uiRootPrefab)
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogError("GameBootstrap not found in Bootstrap scene.");
                return;
            }

            var gameManager = bootstrap.GetComponent<GameManager>();
            if (gameManager.GetComponent<GameSessionManager>() == null)
            {
                bootstrap.gameObject.AddComponent<GameSessionManager>();
            }

            SetSerializedReference(gameManager, "_uiRootPrefab", uiRootPrefab);
            SetSerializedReference(bootstrap, "_initialScene", Core.GameScene.MainMenu);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void WireGameplayScene()
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var levelRoot = GameObject.Find("LevelRoot");
            if (levelRoot == null)
            {
                Debug.LogWarning("LevelRoot not found in Gameplay scene.");
                return;
            }

            if (levelRoot.GetComponent<LevelCompleteMonitor>() == null)
            {
                var monitor = levelRoot.AddComponent<LevelCompleteMonitor>();
                var platform = levelRoot.transform.Find("Platform");
                SetSerializedReference(monitor, "_platform", platform);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetSerializedReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedReference(Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedReference(Object target, string propertyName, Core.GameScene value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).enumValueIndex = (int)value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class PlaceholderClips
        {
            public AudioClip BackgroundMusic;
            public AudioClip ButtonClick;
            public AudioClip ProjectileFire;
            public AudioClip Impact;
            public AudioClip Victory;
            public AudioClip PopupOpen;
            public AudioClip PopupClose;
        }
    }

    internal static class CanvasScalerPortraitExtensions
    {
        public static void ConfigurePortrait(this CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }
}
