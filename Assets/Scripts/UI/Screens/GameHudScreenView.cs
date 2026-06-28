using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    public sealed class GameHudScreenView : UIScreenView
    {
        [SerializeField] private Button _pauseButton;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private TextMeshProUGUI _coinsLabel;

        private UIManager _uiManager;

        private void Awake()
        {
            if (_coinsLabel != null)
            {
                _coinsLabel.text = "0";
            }
        }

        private void Start()
        {
            _uiManager = ServiceLocator.Get<UIManager>();

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        private void OnDestroy()
        {
            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveListener(OnPauseClicked);
            }
        }

        public void SetLevelLabel(string label)
        {
            if (_levelLabel != null)
            {
                _levelLabel.text = label;
            }
        }

        private void OnPauseClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            _uiManager.ShowPauseMenu();
        }
    }
}
