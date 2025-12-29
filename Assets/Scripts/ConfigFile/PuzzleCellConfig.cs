
using System;
using UnityEngine;
[Serializable]
public class PuzzleCellRecord
{
    [SerializeField]
    private int id;
    [SerializeField]
    private int patternId;     // để group
    [SerializeField]
    private int x;
    [SerializeField]
    private int y;
    [SerializeField]
    private int blockId;
    [SerializeField]
    private int screwRequired;

    public int Id { get => id; set => id = value; }
    public int PatternId { get => patternId; set => patternId = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public int BlockId { get => blockId; set => blockId = value; }
    public int ScrewRequired { get => screwRequired; set => screwRequired = value; }
}

public class PuzzleCellConfig : BYDataTable<PuzzleCellRecord>
{
    public override ConfigCompare<PuzzleCellRecord> DefineConfigCompare()
    {
        return new ConfigCompare<PuzzleCellRecord>("id");
    }
  

}