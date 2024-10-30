using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript
{
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

        public LevelItem()
        {
        
        }
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
            var sprite = SpriteLibControl.Instance.GetSpriteByName($"level_{type.ToString()}");
            if (sprite == null) return;
            levelBG.sprite = sprite;

            bool isLockLevel = IsLockLevel(type);
            if (isLockLevel)
            {
                imageIcon.gameObject.SetActive(true);
                textLevel.gameObject.SetActive(false);
                return;
            }

            imageIcon.gameObject.SetActive(false);
            textLevel.gameObject.SetActive(true);
            SetLevelText(id);
            button.onClick.AddListener(() => OnLevelButtonClick(id));
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
            LevelManager.Instance.LoadLevel(id);
        }

        private void OnDisable()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}