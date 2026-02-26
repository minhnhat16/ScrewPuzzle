using Core.Match;

public class ColorMatchRule : IMatchRule
{
    public int RequiredCount => 3;

    public bool IsContainerComplete(IMatchContainer c)
        => c.Count >= RequiredCount;

    public bool CanAccept(IMatchContainer container, IMatchItem item)
        => container.AcceptedTag == item.Tag && !container.IsFull;


    public bool IsComplete(IMatchContainer container) => container.Count >= RequiredCount;
   
}