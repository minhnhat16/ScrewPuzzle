public class InteractionService : IInteractionService
{
    public InteractionMode CurrentMode { get; private set; }

    public void SetMode(InteractionMode mode)
    {
        CurrentMode = mode;
    }

    public void ResetMode()
    {
        CurrentMode = InteractionMode.Normal;
    }
}