using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    public sealed class SettingsPopupView : UIPopupView
    {
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Toggle _sfxToggle;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Button _closeButton;

        private AudioManager _audioManager;
        private bool _suppressEvents;

        private void Start()
        {
            _audioManager = ServiceLocator.Get<AudioManager>();
            BindControls();
            RefreshFromAudioManager();
        }

        private void BindControls()
        {
            if (_musicToggle != null)
            {
                _musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
            }

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (_sfxToggle != null)
            {
                _sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void RefreshFromAudioManager()
        {
            _suppressEvents = true;
            if (_musicToggle != null)
            {
                _musicToggle.isOn = _audioManager.MusicEnabled;
            }

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.value = _audioManager.MusicVolume;
            }

            if (_sfxToggle != null)
            {
                _sfxToggle.isOn = _audioManager.SfxEnabled;
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = _audioManager.SfxVolume;
            }

            _suppressEvents = false;
        }

        private void OnMusicToggleChanged(bool enabled)
        {
            if (_suppressEvents)
            {
                return;
            }

            _audioManager.SetMusicEnabled(enabled);
        }

        private void OnMusicVolumeChanged(float volume)
        {
            if (_suppressEvents)
            {
                return;
            }

            _audioManager.SetMusicVolume(volume);
        }

        private void OnSfxToggleChanged(bool enabled)
        {
            if (_suppressEvents)
            {
                return;
            }

            _audioManager.SetSfxEnabled(enabled);
        }

        private void OnSfxVolumeChanged(float volume)
        {
            if (_suppressEvents)
            {
                return;
            }

            _audioManager.SetSfxVolume(volume);
        }

        private void OnCloseClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            ServiceLocator.Get<PopupManager>().ClosePopup(UIPopupIds.Settings);
        }
    }
}
