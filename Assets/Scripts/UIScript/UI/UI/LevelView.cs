using System;
using System.Collections.Generic;
using System.DataBase;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.UI.UI
{
    public class LevelView : BaseView
    {
        [SerializeField] private int currentLevel = 0;
        [SerializeField]private List<LevelItem> levelItems;
        [SerializeField] private Button closeButton;
        public override void OnStartShowView()
        {
            closeButton.onClick.AddListener(CloseView);
        }
        public override void OnEndHideView()
        {
            closeButton.onClick.RemoveListener(CloseView);
            foreach (LevelItem item in levelItems)
            {
                item.gameObject.SetActive(false);
            }
            
        }
        public override void Setup(ViewParam param)
        {
            if (param == null) return;

            LevelParam newParam = (LevelParam)param;

            currentLevel = newParam.currentLevel;  
            levelItems = newParam.listLevelItems;
            InitListLevelItem(levelItems);
        }

        private void InitListLevelItem( List<LevelItem> items)
        {
            List<LevelItem> newItems = new();
            foreach (var level in items)
            {
                var levelOnPool = LevelItemPool.Instance.pool.SpawnNonGravity();
                levelOnPool.IDLevel = level.IDLevel;
                levelOnPool.IsCompleted  = level.IsCompleted;
                levelOnPool.Init();
                newItems.Add(levelOnPool);
            }
            levelItems = newItems;
        }

        private void CloseView()
        {
            MainScreenViewParam param = new();
            param.totalGold = DataAPIController.instance.GetGold(); 
            ViewManager.Instance.SwitchView(ViewIndex.MainScreenView,param);
        }
    }
}