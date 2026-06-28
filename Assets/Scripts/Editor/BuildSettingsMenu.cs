using ImpactRush.Core;
using UnityEditor;
using UnityEngine;

namespace ImpactRush.Editor
{
    /// <summary>
    /// Editor utilities for validating project bootstrap configuration.
    /// </summary>
    public static class BuildSettingsMenu
    {
        private static readonly string[] RequiredScenes =
        {
            $"Assets/Scenes/{SceneNames.Bootstrap}.unity",
            $"Assets/Scenes/{SceneNames.MainMenu}.unity",
            $"Assets/Scenes/{SceneNames.Gameplay}.unity"
        };

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
