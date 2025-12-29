using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PuzzlePartenRecord
{
    [SerializeField]
    public int patternId;
}
[Serializable]

public class PuzzleParternConfig : BYDataTable<PuzzlePartenRecord>
{
    public override ConfigCompare<PuzzlePartenRecord> DefineConfigCompare()
    {
        return new ConfigCompare<PuzzlePartenRecord>("patternId");
    }
}
