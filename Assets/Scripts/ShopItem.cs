using ConfigFile;
using System.Linq;
using UIScript;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.U2D.ScriptablePacker;

public class ShopItem : PackItem
{

    public override void Init(PackConfigRecord packConfig)
    {
        this.packData = packConfig;
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() =>
        {

            Debug.Log("Pack item on clicked");
            OnBuyClicked?.Invoke(packData);
        });
        // Set pack title + price
        if (ribbonText != null) ribbonText.text = packConfig.Name;
        priceText.text = GameUtils.FormatPrice(packConfig.Price);

        // Clear old items
        foreach (Transform child in ItemContainer)
            child.gameObject.SetActive(false);

        var itemConfig = packConfig.Items;

        var item = itemConfig.FirstOrDefault();
        PackMiniItem miniItem;
        miniItem = Instantiate(itemPrefab, ItemContainer).GetComponent<PackMiniItem>();
        Sprite sprite = SpriteLibControl.Instance.GetSpriteByName(item.Id.ToString());
        miniItem.Init(item.Id, item.Quantity, sprite);
        return;
        // Spawn each item inside the bundle

    }


}
