using UnityEngine;

namespace ImpactRush.UI
{
    /// <summary>
    /// Base behaviour for full-screen UI layers managed by <see cref="UIManager"/>.
    /// </summary>
    public abstract class UIScreenView : MonoBehaviour, IUIScreen
    {
        [SerializeField] private string _screenId;

        public string ScreenId => string.IsNullOrWhiteSpace(_screenId) ? gameObject.name : _screenId;

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
