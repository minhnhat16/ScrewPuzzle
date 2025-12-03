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
            var  baseConfig= LevelManager.ins.levelConfig;
            List<BaseLevelItem> listLevel = new();
            var data = DataAPIController.instance.GetAllLevelData();


            baseConfig = baseConfig.OrderBy(level => level.levelId).ToList();
            foreach (var levelConfig in baseConfig)
            {
                int id = levelConfig.levelId;
                var currentLevel = data.Find((data) => data.levelID == id);
                bool isComplete = currentLevel?.isCompleted == true;
                BaseLevelItem newItem = new (id, isComplete, false);
                listLevel.Add(newItem);
            }
            baseLevelItems= listLevel;
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
                levelOnPool.IDLevel = level.IdLevel;
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