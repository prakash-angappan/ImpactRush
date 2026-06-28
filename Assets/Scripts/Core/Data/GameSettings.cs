using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Core.Data
{
    /// <summary>
    /// Global runtime configuration. No gameplay tuning — application and audio defaults only.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Impact Rush/Game Settings")]
    public sealed class GameSettings : ScriptableObject
    {
        [SerializeField] private int _targetFps = Constants.DefaultTargetFps;
        [SerializeField] private float _fixedTimestep = Constants.DefaultFixedTimestep;
        [SerializeField, Range(0f, 1f)] private float _masterVolume = Constants.DefaultMasterVolume;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = Constants.DefaultMusicVolume;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = Constants.DefaultSfxVolume;

        public int TargetFps => _targetFps;
        public float FixedTimestep => _fixedTimestep;
        public float MasterVolume => _masterVolume.ClampVolume();
        public float MusicVolume => _musicVolume.ClampVolume();
        public float SfxVolume => _sfxVolume.ClampVolume();
    }
}
