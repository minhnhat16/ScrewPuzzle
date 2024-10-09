using UnityEngine;

namespace Level
{
    public class SpriteNameUpdater : MonoBehaviour
    {
        private SpriteChangeNotifier _spriteChangeNotifier;

        private void Start()
        {
            // Get the SpriteChangeNotifier component and subscribe to the event
            _spriteChangeNotifier = GetComponent<SpriteChangeNotifier>();

            if (_spriteChangeNotifier != null)
            {
                _spriteChangeNotifier.OnSpriteChanged.AddListener(UpdateObjectName);
            }
            else
            {
                Debug.LogError("SpriteChangeNotifier not found on the object.");
            }
        }

        // This method will be called when the sprite changes
        private void UpdateObjectName(Sprite newSprite)
        {
            if (newSprite != null)
            {
                gameObject.name = newSprite.name; // Update the object's name to the sprite's name
                Debug.Log("Object's name updated to: " + newSprite.name);
            }
        }
    }
}