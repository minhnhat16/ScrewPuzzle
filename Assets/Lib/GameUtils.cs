using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
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
public static class GameViewUtils
{
    public static void SetGameViewSize(int width, int height)
    {
        var group = GetCurrentGroupType();
        int index = FindSize(group, width, height);

        if (index == -1)
        {
            AddCustomSize(group, width, height);
            index = FindSize(group, width, height);
        }

        SetSize(index);
    }

    // ====================== INTERNAL IMPLEMENTATION ======================

    private static Type gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
    private static Type gameViewSizeGroupType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeGroupType");
    private static Type gameViewSizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
    private static Type scriptableSingletonType = typeof(Editor).Assembly.GetType("UnityEditor.ScriptableSingleton`1")
        .MakeGenericType(gameViewSizesType);

    private static object GetGameViewSizesInstance()
    {
        return scriptableSingletonType.GetProperty("instance").GetValue(null, null);
    }

    private static object GetGroup(object instance, int groupType)
    {
        return gameViewSizesType.GetMethod("GetGroup").Invoke(instance, new object[] { groupType });
    }

    private static int GetCurrentGroupType()
    {
        return (int)gameViewSizeGroupType.GetEnumValues().GetValue(0); // Standalone
    }

    private static int FindSize(int groupType, int width, int height)
    {
        var instance = GetGameViewSizesInstance();
        var group = GetGroup(instance, groupType);

        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        var texts = getDisplayTexts.Invoke(group, null) as string[];

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].Contains($"{width} x {height}"))
                return i;
        }
        return -1;
    }

    private static void AddCustomSize(int groupType, int width, int height)
    {
        var instance = GetGameViewSizesInstance();
        var group = GetGroup(instance, groupType);

        var sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        var ctor = gameViewSizeType.GetConstructor(new Type[] { sizeType, typeof(int), typeof(int), typeof(string) });

        var newSize = ctor.Invoke(new object[]
        {
            Enum.Parse(sizeType, "FixedResolution"),
            width,
            height,
            $"{width}x{height}"
        });

        var addCustom = group.GetType().GetMethod("AddCustomSize");
        addCustom.Invoke(group, new object[] { newSize });
    }

    private static void SetSize(int index)
    {
        var gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        var prop = gameView.GetType().GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        prop.SetValue(gameView, index, null);
    }


    public static void SetGameViewResolution(int width, int height)
    {
        // Lấy type GameView
        var gvType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var gvWindow = EditorWindow.GetWindow(gvType);
        if (gvWindow == null)
        {
            Debug.LogError("Cannot open GameView!");
            return;
        }

        // Lấy GameViewState
        var gvStateType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewState");
        if (gvStateType == null)
        {
            Debug.LogError("GameViewState Type Not Found (Unity changed internal API).");
            return;
        }

        // Lấy field private: m_GameViewState
        var field = gvType.GetField("m_GameViewState", BindingFlags.NonPublic | BindingFlags.Instance);
        var gvState = field.GetValue(gvWindow);
        if (gvState == null)
        {
            Debug.LogError("GameViewState is NULL!");
            return;
        }

        // Lấy property: targetSize
        var sizeProp = gvStateType.GetProperty("targetSize", BindingFlags.Public | BindingFlags.Instance);

        if (sizeProp == null)
        {
            Debug.LogError("targetSize property not found!");
            return;
        }

        // Set GameView resolution
        var newSize = new Vector2(width, height);
        sizeProp.SetValue(gvState, newSize);
        gvWindow.Repaint();

        Debug.Log($"GameView Resolution Set To: {width} x {height}");
    }
}


public static class ChestTierHelper
{
    public static string GetSpriteName(ChestTier tier)
    {
        switch (tier)
        {
            case ChestTier.Common: return "chest_common";
            case ChestTier.Rare: return "chest_rare";
            case ChestTier.Epic: return "chest_epic";
            case ChestTier.Legendary: return "chest_legendary";
            case ChestTier.Special: return "chest_special";
            //case ChestTier.Premium: return "chest_premium";
            //case ChestTier.Mega: return "chest_mega";
            default: return "chest_common";
        }
    }
}
