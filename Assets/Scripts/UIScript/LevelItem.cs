using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript
{
    [Serializable]
    public class BaseLevelItem {
        [SerializeField] private int idLevel;
        [SerializeField] private bool isCompleted;
        [SerializeField] private bool isHardLevel;

        public bool IsHardLevel { get => isHardLevel; set => isHardLevel = value; }
        public bool IsCompleted { get => isCompleted; set => isCompleted = value; }
        public int IdLevel { get => idLevel; set => idLevel = value; }
        public BaseLevelItem(int idLevel, bool isCompleted, bool isHardLevel)
        {
            this.idLevel = idLevel;
            this.isCompleted = isCompleted;
            this.isHardLevel = isHardLevel;
        }

    }

    public class LevelItem : MonoBehaviour
    {
        [SerializeField] private int idLevel;
        [SerializeField] private bool isCompleted;
        [SerializeField] private bool isHardLevel;

        public bool IsHardLevel
        {
            get => isHardLevel;
            set => isHardLevel = value;
        }

        [SerializeField] private int levelStars;

        [SerializeField] private LevelEnum levelType;

 
        //UI COMPONENT
        [SerializeField] private Image levelBG;
        [SerializeField] private Image imageIcon;
        [SerializeField] private Text textLevel;
        [SerializeField] private Button button;
        public LevelItem(int idLevel, bool isCompleted, bool isHardLevel)
        {
            this.idLevel = idLevel;
            this.isCompleted = isCompleted;
            this.isHardLevel = isHardLevel;
        }

        public int IDLevel
        {
            get => idLevel;
            set => idLevel = value;
        }

        public bool IsCompleted
        {
            get => isCompleted;
            set => isCompleted = value;
        }

        public int LevelStart
        {
            get => levelStars;
            set => levelStars = value;
        }
        public LevelEnum LevelType
        {
            get => levelType;
            set => levelType = value;
        }


        private void OnEnable()
        {
            button.onClick.AddListener(() => OnLevelButtonClick(idLevel));

        }

        private void OnDisable()
        {
            button.onClick.RemoveAllListeners();

        }
        public void Setup(int idLevel, bool isCompleted, int levelStars,LevelEnum levelType)
        {
            this.idLevel = idLevel;
            this.isCompleted = isCompleted;
            this.levelStars = levelStars;
            this.levelType = levelType;
        }

        internal void Init()
        {
            SetTypeLevel(!isCompleted, isHardLevel);
            SetLevelSpriteByType(idLevel, levelType);
        }

        private void SetLevelText(int id)
        {
            id++;
            textLevel.text = id.ToString();
        }

        private void SetTypeLevel(bool isLocked, bool isHardLevel)
        {
            //Debug.LogWarning("Set type level " + isLocked);
            if (isLocked )
            {
                levelType = LevelEnum.Lock;
                return;
            }

            if (isHardLevel)
            {
                levelType = LevelEnum.Hard;
                return;
            }
            levelType = LevelEnum.Complete;
            return;
        }
        private void SetLevelSpriteByType(int id, LevelEnum type)
        {
            // Get the sprite based on the level type
            var sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, $"level_{type.ToString()}");
            if (sprite == null) return;
            levelBG.sprite = sprite;

            // Check if the level is locked
            bool isLockLevel = IsLockLevel(type);

            if (isLockLevel)
            {
                // Show imageIcon by adjusting its alpha or enabling its CanvasRenderer
                imageIcon.color = new Color(imageIcon.color.r, imageIcon.color.g, imageIcon.color.b, 1); // Fully visible
                imageIcon.raycastTarget = false;

                // Hide textLevel by adjusting its alpha or disabling its CanvasRenderer
                textLevel.color = new Color(textLevel.color.r, textLevel.color.g, textLevel.color.b, 0); // Fully transparent
                textLevel.raycastTarget = false;

                return;
            }

            // If not locked, show the text and hide the icon
            imageIcon.color = new Color(imageIcon.color.r, imageIcon.color.g, imageIcon.color.b, 0); // Fully transparent
            imageIcon.raycastTarget = true;

            textLevel.color = new Color(textLevel.color.r, textLevel.color.g, textLevel.color.b, 1); // Fully visible
            textLevel.raycastTarget = true;

            // Set level text and add click listener
            SetLevelText(id);
        }


        private bool IsLockLevel(LevelEnum type)
        {
            return type == LevelEnum.Lock;
        }
        private void OnLevelButtonClick(int id)
        {
            Debug.Log("Level " + id + " clicked!");
            HandleLevelClicked(id);
        }

        // Custom logic when level is clicked
        private void HandleLevelClicked(int id)
        {
            // Perform actions when the level button is clicked (e.g., load the level)
            Debug.Log("Handle logic for level: " + id);
            LevelManager.ins.LoadLevel(id);
        }

     
    }
}