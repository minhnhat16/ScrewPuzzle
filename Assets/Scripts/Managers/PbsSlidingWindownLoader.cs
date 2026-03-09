using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PSB Sliding Window theo Chapter.
/// Rule: 5 levels = 1 chapter.
/// Labels trong Addressables:
///   - Per chapter: "Chapter_01", "Chapter_02", ...  (format: "Chapter_XX")
///   - Per level:   "Level_001", "Level_002", ...    (format: "Level_XXX")
///
/// Window: load chapter hiện tại + chapter kế tiếp (preload nền),
///         unload chapter cách 2+ chapter về trước.
/// </summary>
public class PsbSlidingWindowLoader : SingletonMono<PsbSlidingWindowLoader>,IResetable
{
    private const int LEVELS_PER_CHAPTER = 5;
    private const int CHAPTERS_AHEAD = 1;
    private const int CHAPTERS_BEHIND = 0;

    private readonly HashSet<int> _loadedChapters = new();
    private readonly HashSet<int> _loadingChapters = new();

    /// <summary>
    /// Gọi khi scene InGame reload để reset trạng thái PSB.
    /// ResourceManager instance mới → spriteDict trống → phải load lại.
    /// </summary>
    public void OnReset()
    {
        _loadedChapters.Clear();
        _loadingChapters.Clear();
        Debug.Log("[PsbSlidingWindow] State reset — will reload PSBs on next EnsureLoaded.");
    }

    public IEnumerator EnsureLoaded(int levelId)
    {
        int chapter = GetChapter(levelId);

        yield return LoadChapterCoroutine(chapter);

        for (int i = 1; i <= CHAPTERS_AHEAD; i++)
        {
            int nextChapter = chapter + i;
            if (!_loadedChapters.Contains(nextChapter) && !_loadingChapters.Contains(nextChapter))
                PreloadChapterAsync(nextChapter);
        }

        UnloadOutsideWindow(chapter);

        Debug.Log($"[PsbSlidingWindow] Level {levelId} (Chapter {chapter}) ready. " +
                  $"Loaded chapters: [{string.Join(", ", _loadedChapters)}]");
    }

    // ─── Load ──────────────────────────────────────────────────────

    private IEnumerator LoadChapterCoroutine(int chapter)
    {
        if (_loadedChapters.Contains(chapter)) yield break;

        while (_loadingChapters.Contains(chapter))
            yield return null;

        if (_loadedChapters.Contains(chapter)) yield break;

        _loadingChapters.Add(chapter);
        string label = GetChapterLabel(chapter);

        Debug.Log($"[PsbSlidingWindow] Loading chapter {chapter} (label='{label}')...");
        var task = ResourceManager.ins.LoadPSB(label);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
            Debug.LogError($"[PsbSlidingWindow] Failed: chapter {chapter}: {task.Exception?.Message}");
        else
            Debug.Log($"[PsbSlidingWindow] Chapter {chapter} loaded.");

        _loadedChapters.Add(chapter);
        _loadingChapters.Remove(chapter);

        // Append PSB sprites vào index — KHÔNG xóa UI sprites đã có
        SpriteLibControl.Instance.AppendPsbSprites();
    }

    private async void PreloadChapterAsync(int chapter)
    {
        if (_loadedChapters.Contains(chapter) || _loadingChapters.Contains(chapter)) return;

        _loadingChapters.Add(chapter);
        string label = GetChapterLabel(chapter);

        Debug.Log($"[PsbSlidingWindow] Preloading chapter {chapter} (background)...");
        await ResourceManager.ins.LoadPSB(label);

        _loadedChapters.Add(chapter);
        _loadingChapters.Remove(chapter);
        Debug.Log($"[PsbSlidingWindow] Preload done: chapter {chapter}.");
    }

    // ─── Unload ────────────────────────────────────────────────────

    private void UnloadOutsideWindow(int currentChapter)
    {
        int minKeep = currentChapter - CHAPTERS_BEHIND;
        int maxKeep = currentChapter + CHAPTERS_AHEAD;

        var toUnload = new List<int>();
        foreach (var c in _loadedChapters)
        {
            if (c < minKeep || c > maxKeep)
                toUnload.Add(c);
        }

        foreach (var c in toUnload)
        {
            ResourceManager.ins.UnloadPSB(GetChapterLabel(c));
            _loadedChapters.Remove(c);
            Debug.Log($"[PsbSlidingWindow] Unloaded chapter {c}.");
        }
    }

    // ─── Helpers ───────────────────────────────────────────────────

    /// <summary>levelId → chapter index (1-based). Level 0–4 = Chapter 1, v.v.</summary>
    public static int GetChapter(int levelId) => (levelId / LEVELS_PER_CHAPTER) + 1;

    /// <summary>chapter → Addressable label. e.g. chapter 3 → "Chapter_03"</summary>
    private static string GetChapterLabel(int chapter) => $"Chapter_{chapter:D2}";
}