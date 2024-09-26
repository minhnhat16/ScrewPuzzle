using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace System
{
    public class SpriteLibControl : MonoBehaviour
    {
        public static SpriteLibControl Instance;

        [SerializeField]  private List<Sprite> sprites;
    

        readonly private Dictionary<string, Sprite> spriteDict = new();

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            /*foreach (var sprite in sprites)
            {
                spriteDict.Add(sprite.name, sprite);
            }*/
        }

        public Sprite GetSpriteByName(string spriteName)
        {
            //Debug.Log($"GetSpriteByName{name}");
            return spriteDict.GetValueOrDefault(spriteName);
        }
   
    
    }
}
