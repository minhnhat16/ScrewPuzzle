using ConfigFile;
using Ingame;
using System;
using System.Collections.Generic;

public interface IBoxFactory
{

    List<Box> Boxes { get; }
    List<Box> CreateBoxes(IEnumerable<BoxConfigRecord> records);
    Box SpawnNext();
    Box SpawnByPredicate(Func<Box, bool> predicate);
}