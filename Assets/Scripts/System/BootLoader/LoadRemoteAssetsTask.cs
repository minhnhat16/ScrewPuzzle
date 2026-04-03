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

        // Index UI/non-PSB sprites vào SpriteLibControl - only essential sprites
        SpriteLibControl.Instance.LoadAllPartSprites(remoteLoad: true);

        // DEFERRED: Shader.WarmupAllShaders() moved to after UI is displayed (see BootLoader.cs)
        // This was causing 2-5+ second freeze on mobile. Warming up in background doesn't impact visual quality.

        Debug.Log("[LoadRemoteAssetsTask] Boot complete. PSB will load per-level via sliding window.");

        // Schedule shader warmup asynchronously after boot completes
        ScheduleShaderWarmup();
    }

    private void ScheduleShaderWarmup()
    {
        // Fire-and-forget: Warm up shaders after a delay so it doesn't block UI
        MonoBehaviour coroutineRunner = ResourceManager.ins;
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(WarmupShadersAsync());
        }
    }

    private IEnumerator WarmupShadersAsync()
    {
        // Wait for UI to be fully displayed before warming up shaders
        yield return new WaitForSeconds(1.0f);

        Debug.Log("[LoadRemoteAssetsTask] Starting deferred shader warmup...");
        float startTime = Time.realtimeSinceStartup;

        Shader.WarmupAllShaders();

        float duration = Time.realtimeSinceStartup - startTime;
        Debug.Log($"[LoadRemoteAssetsTask] Shader warmup completed in {duration:F2}s");
    }
}