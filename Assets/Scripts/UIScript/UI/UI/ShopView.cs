using ConfigFile;
using JetBrains.Annotations;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using System.Runtime.CompilerServices;
using UIScript;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static GameUtils;
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


        [SerializeField] private ShopPrefabDatabase prefabDB;


        public ShopController shopController;
        public override void OnInit(Action callback)
        {
            shopController = new ShopController();
            StartCoroutine(InitView(callback));
        }

        IEnumerator InitView(Action callback)
        {
            prefabDB = ConfigFileManager.Instance.GetConfig<ShopPrefabDatabase>();
            var config = ConfigFileManager.Instance.GetAllPackConfig();
            var packs = config.Where(p => p.Id == PackEnum.Pack).ToList();
            yield return null;

            ShopItemLoader.LoadItems(
                packs,
                packRectTransform,
                cfg => prefabDB.GetPrefab(cfg.Pack),
                (item, cfg) => item.Init(cfg),
                itemUI => shopController.Register(itemUI)
            );

            var ticket = config.Where(p => p.Id == PackEnum.Ticket).ToList();
            yield return null;
            ShopItemLoader.LoadItems(
                   ticket,
                   ticketRectTransform,
                   cfg => prefabDB.GetPrefab(cfg.Pack),
                   (item, cfg) => item.Init(cfg),
                   itemUI => shopController.Register(itemUI)
               );


            Debug.Log("tickets instantiated " + ticketRectTransform.childCount);
            var coins = config.Where(p => p.Id == PackEnum.Coin).ToList();
            yield return null;

            ShopItemLoader.LoadItems(
                  coins,
                  coinRectTransform,
                  cfg => prefabDB.GetPrefab(cfg.Pack),
                  (item, cfg) => item.Init(cfg),
                  itemUI => shopController.Register(itemUI)
              );
            Debug.Log("coins instantiated " + ticketRectTransform.childCount);

            callback?.Invoke();
        }

    }

}

public class ShopController
{
    private bool isProcessing = false;

    public void Register(PackItem item)
    {
        item.OnBuyClicked += OnBuyClicked;
    }

    private void OnBuyClicked(PackConfigRecord config)
    {
        if (isProcessing)
            return; // tránh spam / double buy

        isProcessing = true;

        PaymentManager.ins.PurchasePack(config);

        // Khi payment done → unlock
        PaymentManager.ins.OnPaymentCompleted += OnPaymentDone;
    }

    private void OnPaymentDone(PaymentResult result)
    {
        isProcessing = false;

        // gỡ để không double-subscribe
        PaymentManager.ins.OnPaymentCompleted -= OnPaymentDone;
    }
}

