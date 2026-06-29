using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    public sealed class MainMenuScreenView : UIScreenView
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TextMeshProUGUI _titleLabel;

        private UIManager _uiManager;
        private GameSessionManager _sessionManager;

        private void Awake()
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = "IMPACT RUSH";
            }
        }

        private void Start()
        {
            _uiManager = ServiceLocator.Get<UIManager>();
            _sessionManager = ServiceLocator.Get<GameSessionManager>();

            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(OnExitClicked);
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(OnExitClicked);
            }
        }

        private void OnPlayClicked()
        {
            PlayClick();
            _sessionManager.LoadGameplay();
        }

        private void OnSettingsClicked()
        {
            PlayClick();
            _uiManager.ShowSettings();
        }

        private void OnExitClicked()
        {
            PlayClick();
            _uiManager.ShowExitConfirmation();
        }

        private static void PlayClick()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
        }
    }
}
