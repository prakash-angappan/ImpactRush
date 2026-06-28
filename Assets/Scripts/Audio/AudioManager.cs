using ImpactRush.Core.Events;
using ImpactRush.Core.Interfaces;
using ImpactRush.Core.Managers;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Audio
{
    /// <summary>
    /// Central audio playback with PlayerPrefs-backed volume and mute settings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour, IGameService, IInitializable
    {
        [SerializeField] private AudioLibrary _library;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        public bool MusicEnabled { get; private set; } = true;
        public float MusicVolume { get; private set; } = Constants.DefaultMusicVolume;
        public bool SfxEnabled { get; private set; } = true;
        public float SfxVolume { get; private set; } = Constants.DefaultSfxVolume;

        public void Initialize()
        {
            Guard.AgainstNull(_library, nameof(_library));
            EnsureSources();

            MusicEnabled = PlayerPrefs.GetInt(Constants.PrefMusicEnabled, 1) == 1;
            MusicVolume = PlayerPrefs.GetFloat(Constants.PrefMusicVolume, Constants.DefaultMusicVolume).ClampVolume();
            SfxEnabled = PlayerPrefs.GetInt(Constants.PrefSfxEnabled, 1) == 1;
            SfxVolume = PlayerPrefs.GetFloat(Constants.PrefSfxVolume, Constants.DefaultSfxVolume).ClampVolume();

            ApplyMusicSettings();
            SubscribeEvents();
            PlayMusic(AudioIds.BackgroundMusic, loop: true);
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        public void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;
            PlayerPrefs.SetInt(Constants.PrefMusicEnabled, enabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMusicSettings();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume.ClampVolume();
            PlayerPrefs.SetFloat(Constants.PrefMusicVolume, MusicVolume);
            PlayerPrefs.Save();
            ApplyMusicSettings();
        }

        public void SetSfxEnabled(bool enabled)
        {
            SfxEnabled = enabled;
            PlayerPrefs.SetInt(Constants.PrefSfxEnabled, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume)
        {
            SfxVolume = volume.ClampVolume();
            PlayerPrefs.SetFloat(Constants.PrefSfxVolume, SfxVolume);
            PlayerPrefs.Save();
        }

        public void PlayMusic(string clipId, bool loop = true)
        {
            if (_musicSource == null || _library == null)
            {
                return;
            }

            var clip = _library.Resolve(clipId);
            if (clip == null)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            ApplyMusicSettings();
            if (MusicEnabled)
            {
                _musicSource.Play();
            }
        }

        public void PlaySfx(string clipId)
        {
            if (!SfxEnabled || _sfxSource == null || _library == null)
            {
                return;
            }

            var clip = _library.Resolve(clipId);
            if (clip == null)
            {
                return;
            }

            _sfxSource.PlayOneShot(clip, SfxVolume);
        }

        private void ApplyMusicSettings()
        {
            if (_musicSource == null)
            {
                return;
            }

            _musicSource.volume = MusicVolume;
            if (!MusicEnabled)
            {
                _musicSource.Stop();
                return;
            }

            if (_musicSource.clip != null && !_musicSource.isPlaying)
            {
                _musicSource.Play();
            }
        }

        private void EnsureSources()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
                _sfxSource.loop = false;
            }
        }

        private void SubscribeEvents()
        {
            EventBus.Subscribe<PlaySfxEvent>(OnPlaySfx);
            EventBus.Subscribe<PlayMusicEvent>(OnPlayMusic);
            EventBus.Subscribe<LevelCompleteDetectedEvent>(OnLevelComplete);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<PlaySfxEvent>(OnPlaySfx);
            EventBus.Unsubscribe<PlayMusicEvent>(OnPlayMusic);
            EventBus.Unsubscribe<LevelCompleteDetectedEvent>(OnLevelComplete);
        }

        private void OnPlaySfx(PlaySfxEvent gameEvent)
        {
            PlaySfx(gameEvent.ClipId);
        }

        private void OnPlayMusic(PlayMusicEvent gameEvent)
        {
            PlayMusic(gameEvent.ClipId, gameEvent.Loop);
        }

        private void OnLevelComplete(LevelCompleteDetectedEvent _)
        {
            PlaySfx(AudioIds.Victory);
        }
    }
}
