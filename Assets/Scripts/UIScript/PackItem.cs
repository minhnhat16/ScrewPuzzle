using Coffee.UIExtensions;
using ConfigFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript
{
    public class PackItem : MonoBehaviour
    {
        private float time;
        private float price;
        private int amount;

        [SerializeField] internal Text amountText;
        [SerializeField] internal Text ribbonText;
        [SerializeField] internal Text priceText;
        [SerializeField] internal Dictionary<string, PackMiniItem> miniItemsDict;
        [SerializeField] internal Button purchaseButton;
        [SerializeField] internal Image itemIcon;
        [SerializeField] internal Image ribon;
        [SerializeField] internal UIParticle particles;
        [SerializeField] private RectTransform itemContainer;

        [Header("UI Feedback")]
        [SerializeField] private GameObject loadingOverlay;   // overlay mờ + spinner khi đang mua
        [SerializeField] private GameObject successOverlay;   // flash xanh / checkmark
        [SerializeField] private GameObject failOverlay;      // flash đỏ / X icon
        [SerializeField] private Text feedbackText;           // dòng chữ "Not enough gold" v.v.
        [SerializeField] private float feedbackDuration = 1.5f;

        public GameObject itemPrefab;

        public int Amount { get => amount; set => amount = value; }
        public float Time { get => time; set => time = value; }
        public float Price { get => price; set => price = value; }
        public Text AmountText { get => amountText; set => amountText = value; }
        public Text RibbonText { get => ribbonText; set => ribbonText = value; }
        public Text PriceText { get => priceText; set => priceText = value; }
        public Dictionary<string, PackMiniItem> MiniItemsDict1 { get => miniItemsDict; set => miniItemsDict = value; }
        public Button PurchaseButton1 { get => purchaseButton; set => purchaseButton = value; }
        public Image ItemIcon1 { get => itemIcon; set => itemIcon = value; }
        public Image Ribon1 { get => ribon; set => ribon = value; }
        public RectTransform ItemContainer { get => itemContainer; set => itemContainer = value; }

        public Action<PackConfigRecord> OnBuyClicked;
        internal PackConfigRecord packData;

        // ─── Lifecycle ─────────────────────────────────────────────

        private void OnEnable()
        {
            if (particles != null)
                particles.SetMaterialDirty();

            purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
            purchaseButton.onClick.AddListener(HandlePurchaseClicked);

            // FIX: Lắng nghe kết quả payment
            if (PaymentManager.ins != null)
                PaymentManager.ins.OnPaymentCompleted += HandlePaymentResult;
        }

        private void OnDisable()
        {
            purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);

            if (PaymentManager.ins != null)
                PaymentManager.ins.OnPaymentCompleted -= HandlePaymentResult;

            // Dọn dẹp UI state khi ẩn đi
            SetLoadingState(false);
            HideAllFeedback();
        }

        // ─── Button Handler ────────────────────────────────────────

        private void HandlePurchaseClicked()
        {
            if (PaymentManager.ins != null && PaymentManager.ins.IsPurchasing)
            {
                Debug.Log("[PackItem] Already purchasing, button tap ignored.");
                return;
            }

            Debug.Log("[PackItem] Purchase clicked: " + packData?.Name);

            // Hiện loading ngay khi bấm — trước khi PaymentManager xử lý
            SetLoadingState(true);
            OnBuyClicked?.Invoke(packData);
        }

        // ─── Payment Result Handler ────────────────────────────────

        private void HandlePaymentResult(PaymentResult result)
        {
            // Chỉ xử lý nếu result thuộc pack này
            if (result.pack != null && result.pack != packData) return;

            SetLoadingState(false);

            if (result.success)
                StartCoroutine(ShowFeedback(successOverlay, "✓ " + result.message));
            else
                StartCoroutine(ShowFeedback(failOverlay, result.message));
        }

        // ─── UI Helpers ────────────────────────────────────────────

        private void SetLoadingState(bool isLoading)
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(isLoading);

            // Disable button khi đang loading để chặn double-tap ở tầng UI
            if (purchaseButton != null)
                purchaseButton.interactable = !isLoading;
        }

        private IEnumerator ShowFeedback(GameObject overlay, string message)
        {
            HideAllFeedback();

            if (overlay != null) overlay.SetActive(true);
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.gameObject.SetActive(true);
            }

            // Nếu thành công → play particle
            if (overlay == successOverlay && particles != null)
                particles.Play();

            yield return new WaitForSeconds(feedbackDuration);

            HideAllFeedback();
        }

        private void HideAllFeedback()
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
            if (successOverlay != null) successOverlay.SetActive(false);
            if (failOverlay != null) failOverlay.SetActive(false);
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        }

        // ─── Init ──────────────────────────────────────────────────

        public void Init(string name, long price, int amount)
        {
            this.ribbonText.name = name;
            this.price = price;
            this.priceText.text = price > 0 ? GameUtils.FormatPrice(price) : "FREE";
            this.amount = amount;
            this.amountText.text = $"x{amount}";
        }

        public virtual void Init(PackConfigRecord packConfig)
        {
            this.packData = packConfig;

            if (ribbonText != null) ribbonText.text = packConfig.Name;
            this.priceText.text = packConfig.Price > 0 ? GameUtils.FormatPrice(packConfig.Price) : "FREE";

            // Destroy children cũ — tránh memory leak
            foreach (Transform child in itemContainer)
                Destroy(child.gameObject);

            var itemConfig = packConfig.Items;
            if (itemConfig.Count < 2)
            {
                var item = itemConfig.FirstOrDefault();
                if (item == null) return;

                var miniItem = Instantiate(itemPrefab, itemContainer).GetComponent<PackMiniItem>();
                itemContainer.sizeDelta = Vector2.one * GameConstants.MINI_SIZE * 1.1f;
                Sprite sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, item.Id.ToString());
                miniItem.Init(item.Id, item.Quantity, sprite);
                return;
            }

            foreach (var item in packConfig.Items)
            {
                var miniItem = Instantiate(itemPrefab, itemContainer).GetComponent<PackMiniItem>();
                miniItem.rectTransform.sizeDelta = Vector2.one * GameConstants.MINI_SIZE;
                Sprite sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, item.Id.ToString());
                miniItem.Init(item.Id, item.Quantity, sprite);
            }
        }
    }
}