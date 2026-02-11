using Level;
using Managers;
using System;
using System.Collections.Generic;
using System.ConfigFile;
using System.DataBase;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.UI.UI
{
    public class LevelView : BaseView
    {
        [SerializeField] private int currentLevel = 0;
        [SerializeField] private List<BaseLevelItem> baseLevelItems = new(50);
        [SerializeField]private List<LevelItem> levelItems = new(50);
        [SerializeField] private Button closeButton;
        [SerializeField] private GridLayoutGroup layout;


        [SerializeField] private LevelParam param;
        public override void OnStartShowView()
        {
            layout.enabled = true;
            closeButton.onClick.AddListener(CloseView);
        }
        public override void OnEndHideView()
        {
            closeButton.onClick.RemoveListener(CloseView);
        }

        public override void OnInit(Action callback = null)
        {
            base.OnInit(callback);

            var baseConfig = LevelManager.ins.levelConfig
                .OrderBy(c => c.levelId)
                .ToList();

            var data = DataAPIController.instance.GetLevelProgress(); // ví dụ
            List<BaseLevelItem> listLevel = new();

            foreach (var levelConfig in baseConfig)
            {
                int id = levelConfig.levelId;
                var progress = data?.Find(d => d.levelID == id);

                bool isComplete = progress?.isCompleted ?? false;

                BaseLevelItem newItem = new(
                    id,
                    isComplete,
                    false // locked / selected tuỳ logic
                );

                listLevel.Add(newItem);
            }
            Debug.Log("Total levels initialized: " + listLevel.Count + " level config count " + baseConfig.Count    );
            baseLevelItems = listLevel;
            InitListLevelItem(listLevel);
        }

        public override void Setup(ViewParam param)
        {
            if (param == null) return;

            LevelParam newParam = (LevelParam)param;
            currentLevel = newParam.currentLevel;
            baseLevelItems = newParam.listLevelItems;
            var orderLevelItems = baseLevelItems.OrderBy(item => item.IdLevel);
            layout.enabled = false;
        }

        private void InitListLevelItem( List<BaseLevelItem> items)
        {
            List<LevelItem> newItems = new();
            foreach (var level in items)
            {
                var levelOnPool = LevelItemPool.Instance.pool.SpawnNonGravity();
                levelOnPool.IDLevel = level.IdLevel ;
                levelOnPool.IsCompleted  = true;
                levelOnPool.Init();
                newItems.Add(levelOnPool);
            }
            levelItems = newItems;
            layout.enabled = true;
        }

        private void CloseView()
        {
            MainScreenViewParam param = new();
            param.totalGold = GameManager.instance.GetPlayerGold(); 
            ViewManager.Instance.SwitchView(ViewIndex.MainScreenView,param);
        }
    }
}