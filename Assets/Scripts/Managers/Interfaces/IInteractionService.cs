public interface IInteractionService
{
    InteractionMode CurrentMode { get; }
    void SetMode(InteractionMode mode);
    void ResetMode();
}