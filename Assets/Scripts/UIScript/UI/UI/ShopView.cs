using ConfigFile;
using Managers;
using System;
using System.Collections;
using System.DataBase;
using System.Linq;
using UIScript;
using UnityEngine;
using UnityEngine.UI;
using static GameUtils;

namespace UIScript.UI.UI
{
    public class ShopView : BaseView
    {
        [SerializeField] private GoldDisplay txt_gold;
        [SerializeField] private GoldDisplay txt_ticket;

        [SerializeField] private RectTransform packRectTransform;
        [SerializeField] private RectTransform coinRectTransform;
        [SerializeField] private RectTransform ticketRectTransform;

        [SerializeField] private ShopPrefabDatabase prefabDB;


        public ShopController shopController;
        private void OnEnable()
        {
            DataTrigger.RegisterValueChange(DataPath.TICKET, OnTicketChanged);
            DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
        }

        private void OnDisable()
        {
            DataTrigger.UnRegisterValueChange(DataPath.TICKET, OnTicketChanged);
            DataTrigger.UnRegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
        }
        public override void OnInit(Action callback)
        {
            shopController = new ShopController();
            StartCoroutine(InitView(callback));
        }

        IEnumerator InitView(Action callback)
        {
            prefabDB = Resources.Load<ShopPrefabDatabase>("Config/ShopPrefabDB");
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
        private void OnTicketChanged(object arg0)
        {
           long ticket = DataAPIController.instance.GetTicket();
            txt_ticket.SetGoldToLable(ticket);

        }

        private void OnGoldChanged(object arg0)
        {
            long gold = DataAPIController.instance.GetGold();
            txt_gold.SetGoldToLable(gold);
        }

        public override void Setup(ViewParam viewParam)
        {
            base.Setup(viewParam);

            long ticket = DataAPIController.instance.GetTicket();
            txt_ticket.SetGoldToLable(ticket);
            long gold = DataAPIController.instance.GetGold();
            txt_gold.SetGoldToLable(gold);

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
        PaymentManager.ins.OnPaymentCompleted -= OnPaymentDone;
        PaymentManager.ins.OnPaymentCompleted += OnPaymentDone;

        Debug.Log("Starting purchase for pack: " + config.Pack);    
        PaymentManager.ins.PurchasePack(config);
    }

    private void OnPaymentDone(PaymentResult result)
    {
        // Luôn unlock
        isProcessing = false;

        // Gỡ sub để không bị leak
        PaymentManager.ins.OnPaymentCompleted -= OnPaymentDone;

        // Kiểm tra lỗi payment
        if (!result.success)
        {
            SoundHelper.PlaySFX(SoundManager.SFX.Shop_Purchase_Fail);
            return;
        }
        SoundHelper.PlaySFX(SoundManager.SFX.Shop_Purchase_Success);
    }
}

