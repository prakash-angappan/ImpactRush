using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    public sealed class LevelCompletePopupView : UIPopupView
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;

        private GameSessionManager _sessionManager;

        private void Awake()
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = "LEVEL COMPLETE";
            }
        }

        private void Start()
        {
            _sessionManager = ServiceLocator.Get<GameSessionManager>();

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_nextLevelButton != null)
            {
                _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnRestartClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            _sessionManager.RestartLevel();
        }

        private void OnNextLevelClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            LoadNextLevelPlaceholder();
        }

        private void LoadNextLevelPlaceholder()
        {
            _sessionManager.AdvanceToNextLevel();
        }

        private void OnMainMenuClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            _sessionManager.LoadMainMenu();
        }
    }
}
