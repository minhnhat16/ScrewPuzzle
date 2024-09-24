using System.Collections.Generic;
using UnityEngine;

public class SpriteLibControl : MonoBehaviour
{
    public static SpriteLibControl Instance;

    [SerializeField]
    private List<Sprite> _sprite;
    

    readonly private Dictionary<string, Sprite> spriteDict = new();

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        foreach (var sprite in _sprite)
        {
            spriteDict.Add(sprite.name, sprite);
        }
    }

    public Sprite GetSpriteByName(string name)
    {
        //Debug.Log($"GetSpriteByName{name}");
        if (spriteDict.ContainsKey(name)) return spriteDict[name];
        else return null;
    }
   
    
}
