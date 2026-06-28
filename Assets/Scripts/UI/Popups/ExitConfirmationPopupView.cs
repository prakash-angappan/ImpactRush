using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    public sealed class ExitConfirmationPopupView : UIPopupView
    {
        [SerializeField] private TextMeshProUGUI _messageLabel;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;

        private PopupManager _popupManager;

        private void Awake()
        {
            if (_messageLabel != null)
            {
                _messageLabel.text = "Are you sure you want to quit?";
            }
        }

        private void Start()
        {
            _popupManager = ServiceLocator.Get<PopupManager>();

            if (_yesButton != null)
            {
                _yesButton.onClick.AddListener(OnYesClicked);
            }

            if (_noButton != null)
            {
                _noButton.onClick.AddListener(OnNoClicked);
            }
        }

        private void OnYesClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
#if UNITY_EDITOR
            Debug.Log("Exit requested. Application quit is suppressed in the Editor.");
#else
            Application.Quit();
#endif
        }

        private void OnNoClicked()
        {
            EventBus.Publish(new PlaySfxEvent(AudioIds.ButtonClick));
            _popupManager.ClosePopup(UIPopupIds.ExitConfirmation);
        }
    }
}
