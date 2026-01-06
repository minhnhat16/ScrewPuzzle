using System;
using System.Collections.Generic;
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

        Debug.Log($"[SpriteLibControl] Remote sprites indexed: {spriteIndex.Count}");
    }

    // ================= ADDRESS PARSER =================

    private void ParseAddressAndAdd(string address, Sprite sprite)
    {
        var parts = address.Split('/');
        if (parts.Length < 2) return;

        int layer = 0;
        SpriteGroup group = SpriteGroup.None;

        // ===== HINH structure =====
        // Sprites/HINH/12/a/xxx.png
        if (parts.Length >= 4 &&
            parts[^4].Equals("HINH", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(parts[^3], out layer);
            group = MapGroupFromFolder(parts[^2]);
        }
        else
        {
            // ===== Non-HINH =====
            group = MapGroupFromPath(parts);
            layer = 0; // global
        }

        AddToIndex(layer, group, sprite);
    }

    // ================= INDEX CORE =================

    private void AddToIndex(int layer, SpriteGroup group, Sprite sprite)
    {
        if (sprite == null) return;

        var key = new SpriteIndexKey
        {
            Layer = layer,
            Group = group,
            Name = NormalizeShapeName(sprite.name)
        };

        // Overwrite là hành vi đúng (remote override local)
        spriteIndex[key] = sprite;


        Debug.Log($"[SpriteLibControl] Indexed sprite: Layer={layer}, Group={group}, Name={key.Name}");
    }

    // ================= LOOKUP =================

    public Sprite GetSprite(int layer, SpriteGroup group, string name)
    {
        var key = new SpriteIndexKey
        {
            Layer = layer,
            Group = group,
            Name = NormalizeShapeName(name)
        };
        spriteIndex.TryGetValue(key, out var sprite);

        Debug.Log($"[SpriteLibControl] Lookup sprite: Layer={layer}, Group={group}, Name={key.Name} and sprite {sprite == null}");

        return sprite;
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

        return folder.ToLower() switch
        {
            "a" => SpriteGroup.Main,
            "b" => SpriteGroup.Outline,
            _ => SpriteGroup.None
        };
    }

    private SpriteGroup MapGroupFromPath(string[] parts)
    {
        foreach (var p in parts)
        {
            switch (p.ToLower())
            {
                case "ui":
                    return SpriteGroup.UI;
                case "effect":
                case "effects":
                    return SpriteGroup.Effect;
                case "background":
                case "bg":
                    return SpriteGroup.Background;
            }
        }

        return SpriteGroup.None;
    }

    // ================= UTIL =================

    private string NormalizeShapeName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        input = input.ToLower();

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
    public int Layer;           // 0 = global
    public SpriteGroup Group;   // Main / Outline / UI / Effect / ...
    public string Name;

    public bool Equals(SpriteIndexKey other)
        => Layer == other.Layer
           && Group == other.Group
           && Name == other.Name;

    public override int GetHashCode()
        => HashCode.Combine(Layer, Group, Name);
}
