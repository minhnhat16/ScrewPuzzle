using Ingame;
using System.Collections.Generic;

public interface IBoxSequenceService
{
    void Load(IEnumerable<Box> boxes);
    Box GetNext();
    bool HasNext();
}