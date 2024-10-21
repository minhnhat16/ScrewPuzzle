using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;

namespace UIScript.Dialog
{
    public class LevelDialog : BaseDialog
    {
        [SerializeField] private LevelItem prefabLevelItem;
        [SerializeField] private List<LevelItem> listLevelItem;
        [SerializeField] private Transform grid;
        public override void OnInit()
        {
            Debug.Log("Init new Level item");
            LoadLevelButton();
            LoadLevelToItem();
        }

        private void LoadLevelButton()
        {
        }

        private void LoadLevelToItem()
        {
            List<LevelItem>levelItemList = new();
            var waitUntil = new WaitUntil(()=> LevelManager.Instance.IsInitDone);
            var allLevelConfig = new List<Level.Level>(LevelManager.Instance.Levels.Values);
            Debug.Log("Init new LEvel item");
            /*var allLevelData = DataAPIController.instance.GetAl   lLevelData();*/
            // foreach (var level in allLevelConfig)
            // {
            //     Debug.Log("Init new LEvel item");
            //     LevelItem newItem = Instantiate(prefabLevelItem, transform);
            //     /*var levelData = allLevelData[level.levelId];*/
            //     newItem.Setup(level.levelId, false, 0);
            //     listLevelItem.Add(newItem);
            // }
            for (int i = 0; i < 10; i++)
            { 
                LevelItem newItem = Instantiate(prefabLevelItem, grid.transform);
            
            }
        }
    }
}
