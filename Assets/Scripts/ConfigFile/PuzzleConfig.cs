using System;
using System.Collections.Generic;

using UnityEngine;




[Serializable]
public class PuzzleBoardRecord 
{
    [SerializeField]
    public int id;
    [SerializeField]
    public int width;
    [SerializeField]
    public int height;
    [SerializeField]
    public int parternId;
  
}

public class PuzzleConfig : BYDataTable<PuzzleBoardRecord>
{
    public override ConfigCompare<PuzzleBoardRecord> DefineConfigCompare()
    {
        return new ConfigCompare<PuzzleBoardRecord> ("id");
    }
  
}
