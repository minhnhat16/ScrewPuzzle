using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CollectionCards : MonoBehaviour
{

    [SerializeField] private List<CollectionItem>collections = new List<CollectionItem>();
    [SerializeField] private RectTransform content;
    [SerializeField] private Slider cardTotalSlider;
    [SerializeField] private Text cardTotalLb;

    private ColorConfig cardColorConfig;
    private ListCardColor listCardColor;
    private CardType cardType;
    public void Init()
    {
        // Wait for both initializations to complete
        Debug.Log("Collection init ");
        // Cache references to frequently accessed properties and methods
        var dataAPI = DataAPIController.instance;
        var configFileManager = ConfigFileManager.Instance;
        cardType = IngameController.instance.CurrentCardType;
        cardColorConfig = configFileManager.ColorConfig;


        var colors = cardColorConfig.GetAllRecord();
        listCardColor = dataAPI.GetDataColorByType(cardType);

        for (int i = 0; i < colors.Count; i++)
        {
            InitializeCollectionItem(colors[i], i);
        }

        // Update card count and fill count

    }

    private void InitializeCollectionItem( ColorConfigRecord color, int index)
    {
        // Instantiate a new CollectionItem and set its properties
        CollectionItem newItem = CollectionItemPool.Instance.pool.SpawnNonGravityWithIndex(index);
        Sprite sprite = SpriteLibControl.Instance.GetCardSpriteByCategory(cardType, index);
        newItem.CardImg.sprite = sprite;
        bool isColorPresent = listCardColor.color.Contains(color.Name);
        newItem.LockSprite(!isColorPresent);
        // Add the new item to the collections list
        collections.Add(newItem);
        // Optionally wait for a frame to ensure this item is initialized before proceeding
        Debug.Log("collection item init done");
    }

    private void CardTotalCount(int owned,int total)
    {
        cardTotalLb.text = $"{owned}/{total}";
    }
    public void FillCount(int dataOwnCard,int totalCard)
    {
        Debug.LogWarning($"(float)dataOwnCard {dataOwnCard} / (float)totalCard {totalCard} ");
        cardTotalSlider.value =(float)dataOwnCard / (float)totalCard;
        CardTotalCount(dataOwnCard, totalCard);
    }
}
