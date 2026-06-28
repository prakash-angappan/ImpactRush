namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Marker contract for future gameplay session lifecycle management.
    /// </summary>
    public interface IGameSession
    {
        bool IsActive { get; }
    }
}
