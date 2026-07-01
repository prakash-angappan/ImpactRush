using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Audio
{
    /// <summary>
    /// Serialized clip references for the placeholder audio set.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Impact Rush/Audio Library")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _projectileFire;
        [SerializeField] private AudioClip _impact;
        [SerializeField] private AudioClip _victory;
        [SerializeField] private AudioClip _popupOpen;
        [SerializeField] private AudioClip _popupClose;

        public AudioClip BackgroundMusic => _backgroundMusic;
        public AudioClip ButtonClick => _buttonClick;
        public AudioClip ProjectileFire => _projectileFire;
        public AudioClip Impact => _impact;
        public AudioClip Victory => _victory;
        public AudioClip PopupOpen => _popupOpen;
        public AudioClip PopupClose => _popupClose;

        public AudioClip Resolve(string clipId)
        {
            return clipId switch
            {
                AudioIds.BackgroundMusic => _backgroundMusic,
                AudioIds.ButtonClick => _buttonClick,
                AudioIds.ProjectileFire => _projectileFire,
                AudioIds.Impact => _impact,
                AudioIds.FragmentImpact => _impact,
                AudioIds.Victory => _victory,
                AudioIds.PopupOpen => _popupOpen,
                AudioIds.PopupClose => _popupClose,
                _ => null,
            };
        }
    }
}
