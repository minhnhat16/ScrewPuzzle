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
        public override void OnInit(Action callback)
        {
            Debug.Log("Init new Level item");
            LoadLevelButton();
            LoadLevelToItem();
            base.OnInit(callback);
        }

        private void LoadLevelButton()
        {
        }

        private void LoadLevelToItem()
        {
            List<LevelItem>levelItemList = new();
            var waitUntil = new WaitUntil(()=> LevelManager.ins.IsInitDone);
            var allLevelConfig = new List<Level.Level>(LevelManager.ins.Levels.Values);
        }
    }
}
