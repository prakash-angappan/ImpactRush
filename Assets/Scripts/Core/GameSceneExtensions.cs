using System;
using ImpactRush.Utilities;

namespace ImpactRush.Core
{
    /// <summary>
    /// Scene enum conversion helpers.
    /// </summary>
    public static class GameSceneExtensions
    {
        public static string ToSceneName(this GameScene scene)
        {
            return scene switch
            {
                GameScene.Bootstrap => "Bootstrap",
                GameScene.MainMenu => "MainMenu",
                GameScene.Gameplay => "Gameplay",
                _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, "Unknown scene identifier.")
            };
        }
    }
}
