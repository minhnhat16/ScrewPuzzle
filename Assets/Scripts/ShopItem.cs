using ConfigFile;
using System.Linq;
using UIScript;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : PackItem
{
    public override void Init(PackConfigRecord packConfig)
    {
        this.packData = packConfig;
        // Set pack title + price
        if (ribbonText != null) ribbonText.text = packConfig.Name;

        if (packConfig.Price > 0)
            priceText.text = GameUtils.FormatPrice(packConfig.Price);
        else
            priceText.text = "FREE";
        // Clear old items
        foreach (Transform child in ItemContainer)
            child.gameObject.SetActive(false);

        var itemConfig = packConfig.Items;

        var item = itemConfig.FirstOrDefault();
        PackMiniItem miniItem;
        miniItem = Instantiate(itemPrefab, ItemContainer).GetComponent<PackMiniItem>();
        Sprite sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, item.Id.ToString());
        miniItem.Init(item.Id, item.Quantity, sprite);
        return;
        // Spawn each item inside the bundle

    }


}
