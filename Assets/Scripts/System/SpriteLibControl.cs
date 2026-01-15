using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

public class SpriteLibControl : MonoBehaviour
{
    public static SpriteLibControl Instance;

    // Index trung tâm
    private readonly Dictionary<SpriteIndexKey, Sprite> spriteIndex = new();

    // ================= UNITY =================

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ================= ENTRY =================

    public void LoadAllPartSprites(bool remoteLoad = false)
    {
        spriteIndex.Clear();

        if (remoteLoad)
            LoadRemote();
        else
            LoadLocal();

        Debug.Log($"[SpriteLibControl] Indexed sprites: {spriteIndex.Count}");
    }

    // ================= LOCAL =================

    private void LoadLocal()
    {
        // Chỉ dành cho sprite dạng HINH (a / b)
        for (int layer = 1; layer <= 40; layer++)
        {
            LoadLocalGroup(layer, SpriteGroup.Main, "a");
            LoadLocalGroup(layer, SpriteGroup.Outline, "b");
        }
    }

    private void LoadLocalGroup(int layer, SpriteGroup group, string folder)
    {
        var sprites = Resources.LoadAll<Sprite>($"Sprites/HINH/{layer}/{folder}");
        if (sprites == null || sprites.Length == 0)
            return;

        foreach (var sprite in sprites)
        {
            AddToIndex(layer, group, sprite);
        }
    }

    // ================= REMOTE =================

    private void LoadRemote()
    {
        // key = address, value = sprite
        var allSprites = ResourceManager.ins.GetAllSprites();

        foreach (var pair in allSprites)
        {
            string address = pair.Key.Replace("\\", "/");
            Sprite sprite = pair.Value;
            if (sprite == null) continue;

            ParseAddressAndAdd(address, sprite);
        }

        var spriteIndexZero = spriteIndex.Keys.Where(k => k.level == 0).ToList();

        foreach (var item in spriteIndexZero)
        {
            Debug.Log($"sprite name {item.Name}");
        }
    }

    // ================= ADDRESS PARSER =================

    private void ParseAddressAndAdd(string address, Sprite sprite)
    {
        var parts = address.Split('/');

        Debug.Log("Address " + address);
        if (parts.Length < 2)
            return;

        int layer = 0;
        SpriteGroup group = SpriteGroup.None;

        // tìm folder "HINH" (không phụ thuộc depth)
        int hinhIndex = Array.FindIndex(
            parts,
            p => p.Equals("HINH", StringComparison.OrdinalIgnoreCase)
        );

        if (hinhIndex >= 0 && parts.Length >= hinhIndex + 3)
        {
            // Sprites/HINH/{layer|random}/{a|b}/xxx.png
            string layerToken = parts[hinhIndex + 1];

            // layer
            if (layerToken.Equals("random", StringComparison.OrdinalIgnoreCase))
                layer = 0;
            else
                int.TryParse(layerToken, out layer);

            // group từ folder a / b

            Debug.Log("token equals to random");
            group = MapGroupFromFolder(parts[hinhIndex + 2]);
        }
        else
        {
            // Non-HINH
            layer = 0;
            group = MapGroupFromPath(parts);
            if (group == SpriteGroup.None)
                group = SpriteGroup.UI;
        }

        AddToIndex(layer, group, sprite);
    }


    // ================= INDEX CORE =================

    private void AddToIndex(int layer, SpriteGroup group, Sprite sprite)
    {
        if (sprite == null) return;

        var key = new SpriteIndexKey
        {
            level = layer,
            Group = group,
            Name = NormalizeShapeName(sprite.name)
        };
        Debug.Log($"[SpriteLibControl] Indexed sprite: Layer={layer}, Group={group}, Name={key.Name}");
        // Overwrite là hành vi đúng (remote override local)
        spriteIndex[key] = sprite;


    }

    // ================= LOOKUP =================
    public Sprite GetSpritePSB(int level, SpriteGroup group, string layer, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        string partname = $"{name}";
        var sprite = ResourceManager.ins.GetSprite(partname);
        return sprite;
    }
    public Sprite GetSprite(int level, SpriteGroup group, string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        string normalized = NormalizeShapeName(name);

        // helper to try lookups using level/group/name key
        bool TryLookup(int l, SpriteGroup g, out Sprite s)
        {
            var k = new SpriteIndexKey { level = l, Group = g, Name = normalized };
            return spriteIndex.TryGetValue(k, out s);
        }

        // 1) exact (level + group + name)
        if (TryLookup(level, group, out var sprite))
        {
            Debug.Log($"[SpriteLibControl] Found exact: L={level}, G={group}, N={normalized}");
            return sprite;
        }

        // 2) fallback: same group but global level (0)
        if (level != 0 && TryLookup(0, group, out sprite))
        {
            Debug.LogWarning($"[SpriteLibControl] Fallback: used global(0) for group {group}, name={normalized}");
            return sprite;
        }

        // 3) fallback: any entry with same group & name (ignore level)
        foreach (var pair in spriteIndex)
        {
            if (pair.Key.Group == group &&
                string.Equals(pair.Key.Name, normalized, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[SpriteLibControl] Fallback: found same group entry at Layer={pair.Key.level}, Group={pair.Key.Group}, Name={pair.Key.Name}");
                return pair.Value;
            }
        }

        // 4) last-resort: any entry with same normalized name
        foreach (var pair in spriteIndex)
        {
            if (string.Equals(pair.Key.Name, normalized, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[SpriteLibControl] Broad fallback: found name='{normalized}' in Layer={pair.Key.level}, Group={pair.Key.Group}");
                return pair.Value;
            }
        }

        Debug.LogError($"[SpriteLibControl] Sprite NOT FOUND: requested Layer={level}, Group={group}, Name={normalized}");
        return null;
    }

    /// <summary>
    /// Helper phổ biến cho HINH
    /// </summary>
    public Sprite GetHinhSprite(int layer, bool outline, string shapeName)
    {
        return GetSprite(
            layer,
            outline ? SpriteGroup.Outline : SpriteGroup.Main,
            shapeName
        );
    }

    // ================= GROUP MAPPING =================

    private SpriteGroup MapGroupFromFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder))
            return SpriteGroup.None;
        Debug.Log("Mapping group folder " + folder);
        return folder.ToLower() switch
        {
            "a" => SpriteGroup.Main,
            "b" => SpriteGroup.Outline,
            _ => SpriteGroup.None
        };
    }

    private SpriteGroup MapGroupFromPath(string[] parts)
    {
        if (parts == null || parts.Length == 0)
            return SpriteGroup.None;

        foreach (var p in parts)
        {
            if (string.IsNullOrWhiteSpace(p))
                continue;

            string lower = p.ToLowerInvariant();

            // use substring matching to handle segments like "ui_icons", "effects_v2", "main_background", etc.
            if (lower.Contains("ui"))
                return SpriteGroup.UI;

            if (lower.Contains("effect") || lower.Contains("effects"))
                return SpriteGroup.Effect;

            if (lower.Contains("background") || lower.Equals("bg") || lower.Contains("_bg") || lower.EndsWith("bg"))
                return SpriteGroup.Background;
        }

        return SpriteGroup.None;
    }

    // ================= UTIL =================

    private string NormalizeShapeName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;


        int idx = input.IndexOf("shape");
        if (idx < 0) return input;

        string rest = input[(idx + 5)..].Trim();
        string number = "";

        foreach (char c in rest)
        {
            if (char.IsDigit(c))
                number += c;
            else
                break;
        }

        return $"shape {number}";
    }
}
public struct SpriteIndexKey : IEquatable<SpriteIndexKey>
{
    public int level;           // 0 = global
    public SpriteGroup Group;   // Main / Outline / UI / Effect / ...
    public string Name;

    public bool Equals(SpriteIndexKey other)
        => level == other.level
           && Group == other.Group
           && Name == other.Name;

    public override int GetHashCode()
        => HashCode.Combine(level, Group, Name);
}
