using System;
using System.Collections;
using System.Collections.Generic;
using System.ConfigFile;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour
{
    //[SerializeField] private GameObject prefab;
    //[SerializeField] private GameObject selectionIcon;
    [SerializeField] private LevelConfig config;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] List<LevelItem> _levelItems;
    [SerializeField] private GameObject levelItemContainer;
    [SerializeField] private Transform iconContainer;
    [SerializeField] 
    private Transform selectionIconParent;
  
    public void Init(Action callback)
    {
        Debug.Log("for init card done");
        InitCouroutine(callback);
    }
    public LevelItem GetLeveItem(int index)
    {
        LevelItem item = _levelItems[index];
        if(item == null) return null;
        return item;  
    }
    public void InitCouroutine(Action callback)
    {
    }
    public void IsScrollRectActive(bool isActive)
    {
        scrollRect.enabled = isActive;
        levelItemContainer.SetActive(true);
    }
}
