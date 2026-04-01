using Enums;
using Ingame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class BoxSelectionService
{
    public Box PickNextBox(
        IBoxSequenceService sequence,
        IReadOnlyList<Box> activeBoxes,
        ITopLayerScrewProvider topLayerProvider,
        IArrayScrew arrayScrew,
        float topLayerMatchChance,
        int smartSpawnLayerDepth,
        float difficultyBias)
    {
        if (sequence == null || !sequence.HasNext())
            return null;

        var activeColors = activeBoxes.Select(b => b.Color).ToHashSet();
        bool skipSmart = Random.value < difficultyBias;

        if (!skipSmart)
        {
            Box smart = TryPickFromArray(sequence, activeColors, arrayScrew);
            if (smart == null)
                smart = TryPickFromTopLayer(sequence, activeColors, topLayerProvider, topLayerMatchChance, smartSpawnLayerDepth);
            if (smart == null)
                smart = TryPickNonDuplicate(sequence, activeColors);
            if (smart == null)
                smart = TryPickMatchingActiveBoxWithLeastScrews(sequence, activeBoxes);

            if (smart != null && activeColors.Contains(smart.Color) && smart.Color != ColorEnum.Rainbow)
            {
                Debug.LogWarning($"[BoxSelectionService] Smart pick color={smart.Color} trùng active → trả lại sequence.");
                sequence.ReturnToFront(smart);
                smart = null;
            }

            if (smart != null)
                return smart;
        }
        else
        {
            Debug.Log($"[BoxSelectionService] DifficultyBias ({difficultyBias:P0}) triggered.");
        }

        var fallback = sequence.GetNext();
        if (fallback != null && activeColors.Contains(fallback.Color) && fallback.Color != ColorEnum.Rainbow)
            Debug.LogWarning($"[BoxSelectionService] Fallback buộc spawn trùng màu={fallback.Color} — không còn lựa chọn.");

        return fallback;
    }

    private Box TryPickMatchingActiveBoxWithLeastScrews(IBoxSequenceService sequence, IReadOnlyList<Box> activeBoxes)
    {
        if (activeBoxes == null || activeBoxes.Count == 0) return null;

        var sortedByLeastScrews = activeBoxes
            .Where(b => b != null && !b.IsLocked && !b.IsFull)
            .OrderByDescending(b => b.RemainingCapacity)
            .ToList();

        foreach (var activeBox in sortedByLeastScrews)
        {
            var box = sequence.TryDequeueMatching(b => b.Color == activeBox.Color);
            if (box != null)
            {
                Debug.Log($"[BoxSelectionService] Tầng 3.5 (LeastScrews) — color={box.Color} remaining={activeBox.RemainingCapacity}.");
                return box;
            }
        }

        return null;
    }

    private Box TryPickFromArray(IBoxSequenceService sequence, HashSet<ColorEnum> activeColors, IArrayScrew arrayScrew)
    {
        if (arrayScrew == null || !arrayScrew.HasAny()) return null;

        var sequenceCounts = sequence.GetColorCounts();
        var arrayCounts = arrayScrew.GetHeldColorCounts();
        if (arrayCounts.Count == 0) return null;

        var candidates = sequenceCounts.Keys
            .Where(c => !activeColors.Contains(c) && arrayCounts.ContainsKey(c))
            .OrderByDescending(c => arrayCounts[c])
            .ToList();

        Debug.Log("[BoxSelectionService] [TryPickFromArray] Candidates: " +
                  $"[{string.Join(", ", candidates)}], Count: {candidates.Count}, ArrayCounts: " +
                  $"[{string.Join(", ", arrayCounts.Select(kv => $"{kv.Key}={kv.Value}"))}]");

        if (candidates.Count == 0)
        {
            candidates = sequenceCounts.Keys
                .Where(c => arrayCounts.ContainsKey(c))
                .OrderByDescending(c => arrayCounts[c])
                .ToList();
        }

        foreach (var color in candidates)
        {
            var box = sequence.TryDequeueMatching(b => b.Color == color);
            if (box != null)
            {
                Debug.Log($"[BoxSelectionService] Tầng 1 (Array) — color={box.Color} arrayCount={arrayCounts[color]}.");
                return box;
            }
        }

        return null;
    }

    private Box TryPickFromTopLayer(
        IBoxSequenceService sequence,
        HashSet<ColorEnum> activeColors,
        ITopLayerScrewProvider topLayerProvider,
        float topLayerMatchChance,
        int smartSpawnLayerDepth)
    {
        if (topLayerProvider == null) return null;
        if (Random.value > topLayerMatchChance) return null;

        var sequenceCounts = sequence.GetColorCounts();
        var topColors = topLayerProvider.GetTopLayerColors(smartSpawnLayerDepth);
        if (topColors.Count == 0) return null;

        var preferred = topColors
            .Where(c => !activeColors.Contains(c) && sequenceCounts.ContainsKey(c))
            .ToHashSet();

        var search = preferred.Count > 0
            ? preferred
            : topColors.Where(c => sequenceCounts.ContainsKey(c)).ToHashSet();

        var box = sequence.TryDequeueMatching(b => search.Contains(b.Color));
        if (box != null)
            Debug.Log($"[BoxSelectionService] Tầng 2 (TopLayer) — color={box.Color}.");

        return box;
    }

    private Box TryPickNonDuplicate(IBoxSequenceService sequence, HashSet<ColorEnum> activeColors)
    {
        var box = sequence.TryDequeueMatching(b => !activeColors.Contains(b.Color));
        if (box != null)
            Debug.Log($"[BoxSelectionService] Tầng 3 (NonDuplicate) — color={box.Color}.");
        return box;
    }
}
