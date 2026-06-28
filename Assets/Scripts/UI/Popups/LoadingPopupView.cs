using TMPro;
using UnityEngine;

namespace ImpactRush.UI
{
    public sealed class LoadingPopupView : UIPopupView
    {
        [SerializeField] private TextMeshProUGUI _messageLabel;

        private void Awake()
        {
            if (_messageLabel != null)
            {
                _messageLabel.text = "Loading...";
            }
        }
    }
}
