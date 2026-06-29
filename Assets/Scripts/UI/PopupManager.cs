using System.Collections.Generic;
using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Interfaces;
using ImpactRush.Core.Managers;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.UI
{
    /// <summary>
    /// Opens, closes, and stacks modal popups.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PopupManager : MonoBehaviour, IGameService, IInitializable
    {
        [SerializeField] private UIPopupView _settingsPopup;
        [SerializeField] private UIPopupView _pausePopup;
        [SerializeField] private UIPopupView _exitConfirmationPopup;
        [SerializeField] private UIPopupView _levelCompletePopup;
        [SerializeField] private UIPopupView _levelFailedPopup;
        [SerializeField] private UIPopupView _loadingPopup;

        private readonly Dictionary<string, UIPopupView> _popups = new();
        private readonly Stack<UIPopupView> _popupStack = new();

        public void Initialize()
        {
            RegisterPopup(_settingsPopup);
            RegisterPopup(_pausePopup);
            RegisterPopup(_exitConfirmationPopup);
            RegisterPopup(_levelCompletePopup);
            RegisterPopup(_levelFailedPopup);
            RegisterPopup(_loadingPopup);
        }

        public void OpenPopup(string popupId)
        {
            if (!_popups.TryGetValue(popupId, out var popup) || popup == null)
            {
                Debug.LogWarning($"Popup not found: {popupId}");
                return;
            }

            if (popup.IsVisible)
            {
                return;
            }

            _popupStack.Push(popup);
            popup.Show();
            EventBus.Publish(new PlaySfxEvent(AudioIds.PopupOpen));
        }

        public void ClosePopup(string popupId)
        {
            if (!_popups.TryGetValue(popupId, out var popup) || popup == null || !popup.IsVisible)
            {
                return;
            }

            ClosePopup(popup);
        }

        public void CloseTopPopup()
        {
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                if (popup != null && popup.IsVisible)
                {
                    ClosePopup(popup);
                    return;
                }
            }
        }

        public void CloseAllPopups()
        {
            foreach (var popup in _popups.Values)
            {
                if (popup != null && popup.IsVisible)
                {
                    popup.Hide();
                }
            }

            _popupStack.Clear();
        }

        private void ClosePopup(UIPopupView popup)
        {
            popup.Hide();
            RemoveFromStack(popup);
            EventBus.Publish(new PlaySfxEvent(AudioIds.PopupClose));
        }

        private void RegisterPopup(UIPopupView popup)
        {
            if (popup == null)
            {
                return;
            }

            popup.gameObject.SetActive(false);
            _popups[popup.PopupId] = popup;
        }

        private void RemoveFromStack(UIPopupView popup)
        {
            if (_popupStack.Count == 0)
            {
                return;
            }

            var buffer = new Stack<UIPopupView>();
            while (_popupStack.Count > 0)
            {
                var item = _popupStack.Pop();
                if (item != popup)
                {
                    buffer.Push(item);
                }
            }

            while (buffer.Count > 0)
            {
                _popupStack.Push(buffer.Pop());
            }
        }
    }
}
