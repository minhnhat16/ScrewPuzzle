using System.Collections.Generic;
using UnityEngine;

public class SpriteLibControl : MonoBehaviour
{
    public static SpriteLibControl Instance;

    [SerializeField] private List<Sprite> sprites;

    private Dictionary<string, Sprite> spriteDict = new();

    private Dictionary<string, Sprite> outLineDict = new();
    private string inputText = ""; // Holds the input text for sprite name
    private Sprite displaySprite = null; // Sprite to display

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Optional: if you want this instance to persist across scenes
        }
        else
        {
            //Destroy(gameObject); // Destroy the duplicate instance to maintain the singleton pattern
        }
    }

    private void Start()
    {

        LoadAllSprites();
        // Initialize the sprite dictionary
        foreach (var sprite in sprites)
        {
            if (sprite == null) continue;

            if (!spriteDict.TryAdd(sprite.name, sprite))
            {
                //Debug.LogWarning($"Start: Failed to add sprite with name '{sprite.name}' to dictionary.");
            }
        }

    }


    private void LoadAllSprites()
    {
        // Load tất cả thư mục cấp 1 trong HINH (1,2,...)
        UnityEngine.Object[] layerFolders = UnityEngine.Resources.LoadAll("Sprites/HINH");
        // Nhưng vì Unity không load thư mục, ta chỉ cần gọi riêng 2 nhánh: a và b

        for (int i = 1; i <= 10; i++) // ví dụ 10 layer
        {
            Sprite[] groupA = UnityEngine.Resources.LoadAll<Sprite>($"Sprites/HINH/{i}/a");
            foreach (var s in groupA)
                spriteDict[s.name] = s;

            Sprite[] groupB = UnityEngine.Resources.LoadAll<Sprite>($"Sprites/HINH/{i}/b");
            foreach (var s in groupB)
                outLineDict[s.name] = s;
        }

        Debug.Log($"Loaded {spriteDict.Count} main sprites and {outLineDict.Count} outlines.");
    }

    public Sprite GetSpriteByName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            Debug.LogWarning("GetSpriteByName: spriteName is null or empty.");
            return null;
        }

        if (spriteDict.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"GetSpriteByName: No sprite found with name '{spriteName}'.");
            return null;
        }
    }

    public Sprite GetSprite(string name, bool outline = false)
    {
        Dictionary<string, Sprite> dict = outline ? outLineDict : spriteDict;

        if (dict.TryGetValue(name, out Sprite s))
            return s;

        Debug.LogWarning($"[SpriteLibControl] Sprite '{name}' not found in {(outline ? "outline" : "main")} dict.");
        return null;
    }
}
