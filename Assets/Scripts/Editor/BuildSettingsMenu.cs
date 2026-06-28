using ImpactRush.Core;
using ImpactRush.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ImpactRush.Editor
{
    /// <summary>
    /// Editor utilities for core project assets and build configuration.
    /// </summary>
    public static class BuildSettingsMenu
    {
        private const string GameSettingsAssetPath = "Assets/ScriptableObjects/GameSettings.asset";

        private static readonly string[] RequiredScenes =
        {
            $"Assets/Scenes/{GameScene.Bootstrap.ToSceneName()}.unity",
            $"Assets/Scenes/{GameScene.MainMenu.ToSceneName()}.unity",
            $"Assets/Scenes/{GameScene.Gameplay.ToSceneName()}.unity"
        };

        [MenuItem("Impact Rush/Create Game Settings Asset")]
        public static void CreateGameSettingsAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameSettings>(GameSettingsAssetPath);
            if (existing != null)
            {
                Debug.Log($"GameSettings asset already exists at {GameSettingsAssetPath}.");
                Selection.activeObject = existing;
                return;
            }

            var settings = ScriptableObject.CreateInstance<GameSettings>();
            AssetDatabase.CreateAsset(settings, GameSettingsAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = settings;
            Debug.Log($"Created GameSettings asset at {GameSettingsAssetPath}.");
        }

        [MenuItem("Impact Rush/Validate Build Settings")]
        public static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            var allPresent = true;

            foreach (var requiredScene in RequiredScenes)
            {
                var found = false;
                foreach (var scene in scenes)
                {
                    if (scene.path == requiredScene && scene.enabled)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"Missing or disabled in Build Settings: {requiredScene}");
                    allPresent = false;
                }
            }

            if (allPresent)
            {
                Debug.Log("Build Settings validation passed.");
            }
        }
    }
}
