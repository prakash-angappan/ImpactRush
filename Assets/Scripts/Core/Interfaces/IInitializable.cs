namespace ImpactRush.Core.Interfaces
{
    /// <summary>
    /// Services that require an explicit initialization pass after registration.
    /// </summary>
    public interface IInitializable
    {
        void Initialize();
    }
}
