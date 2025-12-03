using System;
using System.Collections.Generic;
using System.Globalization;
using UIScript;
using UnityEditor;
using UnityEngine;

public static class GameConstants
{
    public const string PLAYER_DATA_KEY = "PLAYER_DATA";
    public const string SOUND_SETTINGS_KEY = "SOUND_SETTINGS";
    public const string MUSIC_SETTINGS_KEY = "MUSIC_SETTINGS";
    public const string VIBRATION_SETTINGS_KEY = "VIBRATION_SETTINGS";
    public const string DAILY_REWARD_KEY = "DAILY_REWARD";
    public const string SPIN_DATA_KEY = "SPIN_DATA";



    public const string SCREW_SPRITE_PATH = "Sprites/GAMEPLAY/DINH";
    public const string BOX_SPRITE_PATH = "Sprites/GAMEPLAY/HOP";

    public static string COMMON_PACK = "Prefabs/UIPrefab/ShopPack/BluePack";
    internal static string BOX_CONFIGS = "Assets/Resources/Config/BoxConfigs/BoxLevel";
    internal static string EPIC_PACK = "Prefabs/UIPrefab/ShopPack/PinkPack";
    internal static string RARE_PACK = "Prefabs/UIPrefab/ShopPack/OrangePack";

    public static float MINI_SIZE = 150f;
}


public static class GameUtils
{
#if UNITY_EDITOR
    public static void LogAndSelect(string message, GameObject go)
    {
        Debug.Log(message, go);                // message with context (clickable in Console)
        if (go == null) return;
        Selection.activeGameObject = go;       // select in Hierarchy / Inspector
        EditorGUIUtility.PingObject(go);      // ping in Project/Hierarchy
        // frame object in Scene view
        if (SceneView.lastActiveSceneView != null)
        {
            // Fix: Frame expects a Bounds, not a GameObject.
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                SceneView.lastActiveSceneView.Frame(renderer.bounds, true);
            }
            else
            {
                // Fallback: frame the object's position with a small bounds
                Bounds bounds = new Bounds(go.transform.position, Vector3.one);
                SceneView.lastActiveSceneView.Frame(bounds, true);
            }
        }
    }
#endif

    public static string FormatPrice(long amount, string currencyCode = "VND")
    {
        var culture = new CultureInfo("vi-VN"); // vùng Việt Nam
        string formatted = string.Format(culture, "{0:N0}", amount);
        return $"{formatted} {currencyCode}";
    }

    public static class ShopItemLoader
    {
        public static void LoadItems<TItemConfig, TItemPrefab>(
         List<TItemConfig> configs,
         RectTransform parent,
         Func<TItemConfig, TItemPrefab> getPrefab,
         Action<TItemPrefab, TItemConfig> onInit = null,
         Action<TItemPrefab> onRegister = null)
         where TItemPrefab : MonoBehaviour
        {
            if (configs == null || parent == null) return;

            foreach (var cfg in configs)
            {
                var prefab = getPrefab(cfg);
                if (prefab == null)
                    continue;

                var itemObj = GameObject.Instantiate(prefab, parent);

                onRegister?.Invoke(itemObj);
                onInit?.Invoke(itemObj, cfg);
            }
        }

    }
}