namespace ImpactRush.UI
{
    /// <summary>
    /// Contract for UI screens managed by <see cref="UIManager"/>.
    /// </summary>
    public interface IUIScreen
    {
        string ScreenId { get; }
        void Show();
        void Hide();
    }
}
