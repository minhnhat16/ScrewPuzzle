using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ConfigFile;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
   public static LevelManager Instance;
   
   public List<BoxConfig> boxconfigsLevel = new List<BoxConfig>(); 
   public void Awake()
   {
       if (Instance != null)
       {
           Instance = this;
       }
   }
   public void Start()
   {
       Init();
   }

   public void Init()
   {
       StartCoroutine(LoadConfigFromFile());
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


}
