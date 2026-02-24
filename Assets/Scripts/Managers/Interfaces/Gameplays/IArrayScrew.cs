using Ingame.Screw;
using System.Collections.Generic;

public interface IArrayScrew
{
    void ProcessScrews(IEnumerable<ScrewController> screws);

    void AppendScrew(ScrewController screw);
    void AppendScrews(IEnumerable<ScrewController> screws);
    void RemoveScrew(ScrewController screw);
    void RemoveScrews(IEnumerable<ScrewController> screws);
}   