using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadRemoteAssetsTask : IBootTask
{
    public string Name => "LoadRemoteAssets";

    public IEnumerator Execute()
    {
        bool done = false;
        Debug.Log("Task load remote asset");

        yield return ResourceManager.ins.Init(
            new List<string> { "level", "UI" },
            () => done = true
        );

        while (!done)
            yield return null;

        SpriteLibControl.Instance.LoadAllPartSprites(true);
        Shader.WarmupAllShaders();
    }
}