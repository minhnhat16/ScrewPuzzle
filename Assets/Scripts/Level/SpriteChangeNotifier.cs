using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    public class SpriteChangeNotifier : MonoBehaviour
    {
        public UnityEvent<Sprite> OnSpriteChanged = new UnityEvent<Sprite>(); // Event to notify when sprite changes
       [SerializeField] private SpriteRenderer _spriteRenderer;
        private Sprite _lastSprite; // Track the previous sprite

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer not found on the object.");
            }
        }

        private void Update()
        {
            // Check if the sprite has changed
            if (_spriteRenderer.sprite != _lastSprite)
            {
                _lastSprite = _spriteRenderer.sprite; // Update the last known sprite

                // Trigger the event and pass the new sprite
                OnSpriteChanged.Invoke(_spriteRenderer.sprite);
            }
        }
    }
}