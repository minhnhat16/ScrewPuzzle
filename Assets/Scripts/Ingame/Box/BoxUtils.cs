

using Enums;
using Ingame;

public static class BoxUtils
{
    public static void SetBoxColor(Box box, ColorEnum colorID)
    {
        if (box == null) return;
        box.SetBoxColor(colorID);
    }
}
