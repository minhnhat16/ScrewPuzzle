using System.Collections;
using UnityEngine;

public class SetupUITask : IBootTask
{
    public string Name => "SetupUI";

    public IEnumerator Execute()
    {
        Debug.Log("[BOOT] Setup UI...");
        // Ensure UI root is enabled by whoever instantiated BootLoader (same as before)
        // Keep identical ordering to previous implementation
        yield return ViewManager.Instance.Init();
        yield return DialogManager.ins.Init();
    }
}