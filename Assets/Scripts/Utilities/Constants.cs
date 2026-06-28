namespace ImpactRush.Utilities
{
    /// <summary>
    /// Project-wide constant values shared across assemblies.
    /// </summary>
    public static class Constants
    {
        public const int DefaultTargetFps = 60;
        public const float DefaultFixedTimestep = 0.02f;
        public const float DefaultMasterVolume = 1f;
        public const float DefaultMusicVolume = 0.8f;
        public const float DefaultSfxVolume = 1f;
        public const float MinVolume = 0f;
        public const float MaxVolume = 1f;

        public const string PrefMusicEnabled = "ImpactRush.MusicEnabled";
        public const string PrefMusicVolume = "ImpactRush.MusicVolume";
        public const string PrefSfxEnabled = "ImpactRush.SfxEnabled";
        public const string PrefSfxVolume = "ImpactRush.SfxVolume";
    }
}
