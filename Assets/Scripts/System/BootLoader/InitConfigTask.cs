using System.Collections;
using UnityEngine;
using System;

public class InitConfigTask : IBootTask
{
    public string Name => "InitConfig";

    public IEnumerator Execute()
    {
        Debug.Log("[BOOT] InitConfig...");
        bool done = false;

        try
        {
            ConfigFileManager.Instance.Init(() =>
            {
                Debug.Log("[BOOT] InitConfig DONE");
                LevelManager.ins.Init(() => done = true);
            });
        }
        catch (Exception e)
        {
            Debug.LogError("[BOOT] InitConfig FAILED\n" + e);
            done = true; // prevent boot hang
        }

        yield return new WaitUntil(() => done);
    }
}   