using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using System.Runtime.CompilerServices;
using ConfigFile;
using Managers;
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
            base.OnInit(callback);
        }


        IEnumerator InitView(Action callback = null)
        {

            yield return null;
            callback?.Invoke();
        }

        public void InitPack(List<PackConfigRecord> packes, Action callback = null) {
            if (packes.Count < 0) return;

            var commonPack = Resources.Load<PackItem>(GameConstants.COMMON_PACK);
            for (int i = 0; i < packes.Count; i++) { 
            
            }
        }
    }
 
}

