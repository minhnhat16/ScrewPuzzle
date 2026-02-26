using Ingame;
using Ingame.Screw;
using System.Collections.Generic;
using System.Linq;

public class ScrewRoutingService
{
    private readonly List<Box> _boxes;
    private readonly List<ScrewController> _hiding;

    public ScrewRoutingService(List<Box> boxes, List<ScrewController> hiding)
    {
        _boxes = boxes;
        _hiding = hiding;
    }
    /// <summary>
    /// Find a suitable box for the given screw based on its color and the box's availability. The method first checks for active boxes that match the screw's color and have free slots. If no active boxes are suitable, it optionally checks inactive boxes if allowed. If no suitable box is found, it returns null.
    /// </summary>
    /// <param name="screw"></param>
    /// <param name="allowInactive"></param>
    /// <returns></returns>
    public Box FindSuitableBox(ScrewController screw, bool allowInactive)
    {
        if (screw == null) return null;

        bool Match(Box box) =>
            box != null &&
            box.Color == screw.GetColor() &&
            !box.IsMoving &&
            !box.IsFull &&
            !box.IsLocked;

        var active = _boxes
            .Where(b => Match(b) && b.gameObject.activeInHierarchy)
            .OrderByDescending(b => 3 -  b.RemainingCapacity)
            .FirstOrDefault();

        if (active != null)
            return active;

        if (!allowInactive)
            return null;

        return _boxes
            .Where(b => Match(b) && !b.gameObject.activeInHierarchy)
            .OrderByDescending(b => 3 - b.RemainingCapacity)
            .FirstOrDefault();
    }

    /// <summary>
    /// Try moving screws to their respective boxes based on color. If a box is full or unavailable, the remaining screws are added to the hiding list.
    /// </summary>
    /// <param name="screws"></param>
    /// <param name="includeInactive"></param>
    /// <returns></returns>
    public bool TryMoveGrouped(List<ScrewController> screws, bool includeInactive)
    {
        if (screws == null || screws.Count == 0)
            return false;

        int totalMoved = 0;

        var groups = screws.Distinct()
                           .Where(s => s != null)
                           .GroupBy(s => s.GetColor());

        foreach (var group in groups)
        {
            var color = group.Key;
            var list = group.ToList();

            var box = FindSuitableBox(list[0], includeInactive);

            if (box == null)
            {
                AddToHiding(list);
                continue;
            }

            var freeSlots = box.RemainingCapacity;

            var toMove = list.Take(freeSlots).ToList();

            box.TryAddScrews(toMove);

            totalMoved += toMove.Count;

            var remain = list.Skip(toMove.Count).ToList();
            if (remain.Count > 0)
                AddToHiding(remain);
        }

        return totalMoved > 0;
    }
    /// <summary>
    /// Temporarily hide screws by adding them to the hiding list and deactivating their game objects. This is used when screws cannot be moved to a suitable box, allowing them to be stored without being visible in the game world.
    /// </summary>
    /// <param name="screws"></param>
    private void AddToHiding(List<ScrewController> screws)
    {
        foreach (var s in screws)
        {
            _hiding.Add(s);
            s.gameObject.SetActive(false);
        }
    }
}