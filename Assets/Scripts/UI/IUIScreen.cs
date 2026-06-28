namespace ImpactRush.UI
{
    /// <summary>
    /// Marker contract for future UI screen lifecycle management.
    /// </summary>
    public interface IUIScreen
    {
        string ScreenId { get; }
        void Show();
        void Hide();
    }
}
