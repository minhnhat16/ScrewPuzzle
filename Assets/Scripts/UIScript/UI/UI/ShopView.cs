using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using System.Runtime.CompilerServices;
using ConfigFile;
using Managers;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;

namespace UIScript.UI.UI
{
    public class ShopView : BaseView
    {
        [SerializeField] private Text txt_gold;
        [SerializeField] private Text txt_ticket;

        [SerializeField] private RectTransform packRectTransform;
        [SerializeField] private RectTransform coinRectTransform;
        [SerializeField] private RectTransform ticketRectTransform;

        public override void OnInit(Action callback)
        {
            StartCoroutine(InitView(callback));
        }


        IEnumerator InitView(Action callback = null)
        {
            var config = ConfigFileManager.Instance.PackConfig.GetAllRecord();
            var packs = config.Where(p => p.Id == PackEnum.Pack).ToList();
            yield return new WaitForSeconds(1f);
            InitPack(packs, callback);
        }

        public void InitPack(List<PackConfigRecord> packes, Action callback = null)
        {
            if (packes == null || packes.Count == 0)
                return;

            var commonPack = Resources.Load<PackItem>(GameConstants.COMMON_PACK);
            var rarePack = Resources.Load<PackItem>(GameConstants.RARE_PACK);
            var epicPack = Resources.Load<PackItem>(GameConstants.EPIC_PACK);

            for (int i = 0; i < packes.Count; i++)
            {
                PackConfigRecord packConfig = packes[i];
                PackItem prefabToSpawn = null;

                switch (packConfig.Pack)
                {
                    case PackType.Common:
                        prefabToSpawn = commonPack;
                        break;

                    case PackType.Rare:
                        prefabToSpawn = rarePack;
                        break;

                    case PackType.Epic:
                        prefabToSpawn = epicPack;
                        break;

                    default:
                        Debug.LogWarning("Unknown pack type: " + packConfig.Pack);
                        continue;
                }
                if (prefabToSpawn != null)
                {
                    PackItem newPack = GameObject.Instantiate(prefabToSpawn, packRectTransform);
                    newPack.Init(packConfig);
                    Debug.Log("Spawned pack: " + newPack.name);
                }
            }

            callback?.Invoke();
        }

    }

}

