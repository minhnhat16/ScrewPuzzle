using System.Collections;
using System.DataBase;
using UnityEngine;

public class InitDataTask : IBootTask
{
    public string Name => "InitData";

    public IEnumerator Execute()
    {
        Debug.Log("[BOOT] InitData...");
        bool done = false;
        DataAPIController.instance.InitData(() => done = true);
        yield return new WaitUntil(() => done);
    }
}