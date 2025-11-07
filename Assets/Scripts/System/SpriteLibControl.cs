using UnityEngine;
using System.Collections.Generic;

namespace System
{
    public class SpriteLibControl : MonoBehaviour
    {
        public static SpriteLibControl Instance;

        [SerializeField] private List<Sprite> sprites;

        private Dictionary<string, Sprite> spriteDict = new();
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

        //private void OnGUI()
        //{
        //    // Create a label and a text field for input
        //    GUI.Label(new Rect(10, 10, 200, 20), "Enter Sprite Name:");
        //    inputText = GUI.TextField(new Rect(10, 40, 200, 20), inputText);

        //    // Create a button; when clicked, it retrieves the sprite by name
        //    if (GUI.Button(new Rect(10, 70, 100, 30), "Get Sprite"))
        //    {
        //        displaySprite = GetSpriteByName(inputText);
        //    }

        //    // Display the sprite if found
        //    if (displaySprite != null)
        //    {
        //        // Ensure the sprite has a texture to display
        //        if (displaySprite.texture != null)
        //        {
        //            // Display the sprite using its texture
        //            GUI.DrawTexture(new Rect(10, 110, 100, 100), displaySprite.texture);
        //        }
        //    }
        //}

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
    }
}
