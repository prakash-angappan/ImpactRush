using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    public sealed class PausePopupView : UIPopupView
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;

        private UIManager _uiManager;
        private GameSessionManager _sessionManager;

        private void Start()
        {
            _uiManager = ServiceLocator.Get<UIManager>();
            _sessionManager = ServiceLocator.Get<GameSessionManager>();

            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnResumeClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            _uiManager.HidePauseMenu();
        }

        private void OnMainMenuClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            _sessionManager.LoadMainMenu();
        }
    }
}
