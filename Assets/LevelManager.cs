using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ConfigFile;
using Ingame;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
   public static LevelManager Instance;
   
   public List<BoxConfig> boxconfigsLevel = new List<BoxConfig>(); 
    public List<GameObject> levelPrefab= new List<GameObject>();

   public void Awake()
   {
       if (Instance != null)
       {
           Instance = this;
       }
       Instance = this;

    }
    public void Start()
   {
       Init();
   }

   public void Init()
   {
        StartCoroutine(LoadLevelOnFile());
        StartCoroutine(LoadConfigFromFile(() =>
        {
        }));
   }

   public IEnumerator LoadConfigFromFile(Action callback = null)
   {
       yield return new WaitForSeconds(1f);

       // Path should not include "Resources" or file extension
       string resourcePath = "Config/Level"; // Assuming files are in Resources/ConfigFile/Level

       // Load all assets of type BoxConfig from the specified folder
       var boxConfigs = Resources.LoadAll<BoxConfig>(resourcePath);

       if (boxConfigs.Length > 0)
       {
           foreach (var boxConfig in boxConfigs)
           {
               boxconfigsLevel.Add(boxConfig);
           }
       }
       else
       {
           Debug.LogError($"No BoxConfig assets found at path: {resourcePath}");
       }

       // Call the callback if it's not null
       callback?.Invoke();
   }

    public IEnumerator LoadLevelOnFile(Action callback = null)
    {
        yield return new WaitForSeconds(1f);

        // Path should not include "Resources" or file extension
        string resourcePath = "Prefabs/Levels"; // Assuming files are in Resources/ConfigFile/Level

        // Load all assets of type BoxConfig from the specified folder
        var levels = Resources.LoadAll<GameObject>(resourcePath);

        if (levels.Length > 0)
        {
            foreach (var boxConfig in levels)
            {
                levelPrefab.Add(boxConfig);
            }
        }
        else
        {
            Debug.LogError($"No GameObject assets found at path: {resourcePath}");
        }

        // Call the callback if it's not null
        callback?.Invoke();
    }
    public IEnumerator LoadLevel(int level, Action callback = null)
    {
        yield return new WaitUntil(() => levelPrefab.Count > 0);
        yield return new WaitForSeconds(1f);
        var leveObject = Instantiate(levelPrefab[level],transform);
        yield return new WaitUntil(()=> leveObject != null);
        var levelScript = leveObject.GetComponent<BaseLevelObject>();
       // levelScript.Init();
        callback?.Invoke();
    }
}
