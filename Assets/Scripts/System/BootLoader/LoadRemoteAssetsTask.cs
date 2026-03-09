using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadRemoteAssetsTask : IBootTask
{
    public string Name => "LoadRemoteAssets";

    public IEnumerator Execute()
    {
        bool done = false;
        Debug.Log("[LoadRemoteAssetsTask] Loading remote assets...");

        // Load level data + UI assets — KHÔNG load PSB ở đây
        // PSB được load lazily theo sliding window khi vào từng level
        yield return ResourceManager.ins.Init(
            new List<string> { "level", "UI" },
            () => done = true
        );

        while (!done) yield return null;

        // Index UI/non-PSB sprites vào SpriteLibControl
        SpriteLibControl.Instance.LoadAllPartSprites(remoteLoad: true);

        Shader.WarmupAllShaders();

        Debug.Log("[LoadRemoteAssetsTask] Boot complete. PSB will load per-level via sliding window.");
    }
}