using System;
using UnityEngine;

[Serializable]
public class ColorConfigRecord
{
    [SerializeField]
    private CardColorPallet name;
    [SerializeField]
    private Color color;
    public CardColorPallet Name { get { return name; } }
    public Color Color { get { return color; } }
}
public class ColorConfig: BYDataTable<ColorConfigRecord>
{
    public override ConfigCompare<ColorConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<ColorConfigRecord>("name");
        return configCompare;
    }
}
