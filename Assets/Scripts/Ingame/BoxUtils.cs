

using Enums;
using Ingame;

public static class BoxUtils
{
    public static void SetBoxColor(ScrewBox box, ColorEnum colorID)
    {
        if (box == null) return;
        box.SetBoxColor(colorID);
    }
}
