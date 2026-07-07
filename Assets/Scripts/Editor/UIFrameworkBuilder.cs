using System.IO;
using ImpactRush.Audio;
using ImpactRush.Core.Bootstrap;
using ImpactRush.Core.Managers;
using ImpactRush.Gameplay;
using ImpactRush.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ImpactRush.Editor
{
    /// <summary>
    /// Builds the reusable UIRoot prefab, placeholder audio, and scene wiring for FEATURE-021.
    /// </summary>
    public static class UIFrameworkBuilder
    {
        private const string UiRootPrefabPath = "Assets/Resources/UI/UIRoot.prefab";
        private const string UiButtonPrefabPath = "Assets/Prefabs/UI/UIButton.prefab";
        private const string DialogBasePrefabPath = "Assets/Prefabs/UI/DialogBase.prefab";
        private const string AudioLibraryPath = "Assets/ScriptableObjects/AudioLibrary.asset";
        private const string AudioFolderPath = "Assets/Resources/Audio";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Impact Rush/Build UI Framework")]
        public static void BuildUIFramework()
        {
            EnsureFolders();
            EnsurePlaceholderSprites();
            UISpriteSheetSlicer.EnsureButtonSpritesSliced();
            DialogBackgroundSlicer.EnsureSliced();
            var theme = LoadThemeAssets();
            var clips = CreateOrLoadPlaceholderClips();
            var library = CreateOrUpdateAudioLibrary(clips);
            var uiRootPrefab = BuildUIRootPrefab(library, theme);
            BuildReusableButtonPrefab(theme);
            BuildReusableDialogPrefab(theme);
            WireBootstrapScene(uiRootPrefab);
            WireGameplayScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UI Framework build completed.");
        }

        [MenuItem("Impact Rush/Clear Baked Target Stack")]
        public static void ClearBakedTargetStackMenu()
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var targetStack = GameObject.Find("LevelRoot/TargetStack")?.transform;
            ClearBakedTargetStackChildren(targetStack);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Removed baked TargetStack children from Gameplay scene.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/UI"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/UI/Placeholders"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/Audio"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/Sprites"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Resources/Fonts"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/Art/UI/Placeholders"));
        }

        private static void EnsurePlaceholderSprites()
        {
            const string folder = "Assets/Resources/UI/Placeholders";
            if (!Directory.Exists(Path.GetFullPath(folder)))
            {
                return;
            }

            foreach (var filePath in Directory.GetFiles(Path.GetFullPath(folder), "*.png"))
            {
                var assetPath = $"{folder}/{Path.GetFileName(filePath).Replace('\\', '/')}";
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                var changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
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
                BackgroundMusic = PlaceholderAudioGenerator.CreateOrLoadBackgroundMusic($"{AudioFolderPath}/BackgroundMusic.wav"),
                ButtonClick = PlaceholderAudioGenerator.CreateOrLoadButtonClick($"{AudioFolderPath}/ButtonClick.wav"),
                ProjectileFire = PlaceholderAudioGenerator.CreateOrLoadProjectileFire($"{AudioFolderPath}/ProjectileFire.wav"),
                Impact = PlaceholderAudioGenerator.CreateOrLoadImpact($"{AudioFolderPath}/Impact.wav"),
                Victory = PlaceholderAudioGenerator.CreateOrLoadVictory($"{AudioFolderPath}/Victory.wav"),
                PopupOpen = PlaceholderAudioGenerator.CreateOrLoadPopupOpen($"{AudioFolderPath}/PopupOpen.wav"),
                PopupClose = PlaceholderAudioGenerator.CreateOrLoadPopupClose($"{AudioFolderPath}/PopupClose.wav"),
            };
        }

        private static GameObject BuildUIRootPrefab(AudioLibrary library, UIThemeAssets theme)
        {
            var root = UIConstruction.BuildUIRootHierarchy(library, theme);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, UiRootPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildReusableButtonPrefab(UIThemeAssets theme)
        {
            Directory.CreateDirectory(Path.GetFullPath("Assets/Prefabs/UI"));
            var button = UIConstruction.BuildButtonTemplate(theme);
            PrefabUtility.SaveAsPrefabAsset(button, UiButtonPrefabPath);
            Object.DestroyImmediate(button);
        }

        private static void BuildReusableDialogPrefab(UIThemeAssets theme)
        {
            Directory.CreateDirectory(Path.GetFullPath("Assets/Prefabs/UI"));
            var dialog = UIConstruction.BuildDialogTemplate(theme);
            PrefabUtility.SaveAsPrefabAsset(dialog, DialogBasePrefabPath);
            Object.DestroyImmediate(dialog);
        }

        private static UIThemeAssets LoadThemeAssets()
        {
            var sprites = LoadSheetSprites();
            var theme = new UIThemeAssets
            {
                MainMenuBackground = LoadMainMenuBackground(),
                DialogBackground = DialogBackgroundSlicer.LoadSprite(),
                PlayIcon = FindSprite(sprites, UISpriteSheetSlicer.PlayNormalName),
                SettingsIcon = FindSprite(sprites, UISpriteSheetSlicer.SettingsNormalName),
                ExitIcon = FindSprite(sprites, UISpriteSheetSlicer.ExitNormalName),
                TrophyIcon = FindSprite(sprites, UISpriteSheetSlicer.TrophyNormalName),
                VolumeIcon = FindSprite(sprites, UISpriteSheetSlicer.VolumeNormalName),
                CloseIcon = FindSprite(sprites, UISpriteSheetSlicer.CloseNormalName),
            };

            var normal = FindSprite(sprites, UISpriteSheetSlicer.ButtonNormalName);
            var hover = FindSprite(sprites, UISpriteSheetSlicer.ButtonHoverName);
            var pressed = FindSprite(sprites, UISpriteSheetSlicer.ButtonPressedName);
            if (normal != null && hover != null && pressed != null)
            {
                theme.Button = new MenuButtonSpriteSet
                {
                    Normal = normal,
                    Hover = hover,
                    Pressed = pressed,
                };
            }
            else
            {
                Debug.LogWarning(
                    "UIFrameworkBuilder: button sprites not found in UISpriteSheet; " +
                    "buttons will fall back to flat colour styling.");
            }

            return theme;
        }

        private static Object[] LoadSheetSprites()
        {
            return AssetDatabase.LoadAllAssetsAtPath(UISpriteSheetSlicer.SpriteSheetPath);
        }

        private static Sprite FindSprite(Object[] assets, string spriteName)
        {
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == spriteName)
                {
                    return sprite;
                }
            }

            Debug.LogWarning($"UIFrameworkBuilder: sprite '{spriteName}' not found in UISpriteSheet.");
            return null;
        }

        private static Sprite LoadMainMenuBackground()
        {
            const string path = "Assets/Art/UI/bg-mainmenu.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning(
                    $"UIFrameworkBuilder: Main Menu background sprite not found at '{path}'; " +
                    "falling back to the flat colour background.");
            }

            return sprite;
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
            SetSerializedReference(bootstrap, "_initialScene", Core.GameScene.Loading);
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

            var targetStack = levelRoot.transform.Find("TargetStack");
            var platform = levelRoot.transform.Find("Platform");
            var monitor = levelRoot.GetComponent<LevelCompleteMonitor>();
            if (monitor == null)
            {
                monitor = levelRoot.AddComponent<LevelCompleteMonitor>();
            }

            SetSerializedReference(monitor, "_platform", platform);
            SetSerializedReference(monitor, "_targetRoot", targetStack);

            if (targetStack != null && targetStack.GetComponent<LevelBuilder>() == null)
            {
                var builder = targetStack.gameObject.AddComponent<LevelBuilder>();
                SetSerializedReference(builder, "_targetStack", targetStack);
                SetMaterialArray(
                    builder,
                    "_targetMaterials",
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/M_WallPrimary.mat"),
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/M_WallSecondary.mat"),
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/M_Cannon.mat"));
            }

            ClearBakedTargetStackChildren(targetStack);

            var camera = Camera.main;
            if (camera != null && camera.GetComponent<GameplayCameraFitter>() == null)
            {
                camera.gameObject.AddComponent<GameplayCameraFitter>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ClearBakedTargetStackChildren(Transform targetStack)
        {
            if (targetStack == null)
            {
                return;
            }

            for (var i = targetStack.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(targetStack.GetChild(i).gameObject);
            }
        }

        private static void SetMaterialArray(Object target, string propertyName, params Material[] materials)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            property.arraySize = materials.Length;
            for (var i = 0; i < materials.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = materials[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
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
}
