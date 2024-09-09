using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour
{
    [SerializeField] private float percent;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private CardType cardType;
    [SerializeField] private Image cardImage;
    [SerializeField] private Text lb_CardName;
    [SerializeField] private Text lb_Percent;
    [SerializeField] private Image percentSlider;
    [SerializeField] private int cardCount;
    [SerializeField] private int totalCard;

    public Image CardImage { get => cardImage; set => cardImage = value; }
    public Text Lb_CardName { get => lb_CardName; set => lb_CardName = value; }
    public Text Lb_Percent { get => lb_Percent; set => lb_Percent = value; }
    public Image PercentSlider { get => percentSlider; set => percentSlider = value; }
    public float Percent { get => percent; set => percent = value; }
    public CardType CardType { get => cardType; set => cardType = value; }
    public int CardCount { get => cardCount; set => cardCount = value; }

    public LevelItem(CardType cardType, Image cardImage, Text lb_CardName, Text lb_Percent, Image percentSlider, float percent, int cardCount)
    {
        this.CardType = cardType;
        this.CardImage = cardImage;
        this.Lb_CardName = lb_CardName;
        this.Lb_Percent = lb_Percent;
        this.PercentSlider = percentSlider;
        this.Percent = percent;
        this.CardCount = cardCount;
    }
    public void Init()
    {
        cardImage.sprite = SpriteLibControl.Instance.GetSpriteByName($"card_{cardType.ToString().ToLower()}");
        lb_CardName.text = $"{(int)CardType + 1}. {CardType}";
        isUnlocked = CheckUnlock();
        percent = PercentCalculator();
        if (isUnlocked)
        {
            percent = PercentCalculator();
            percentSlider.fillAmount = percent / 100;

            lb_Percent.text = $"{percent}%";
        }
        else
        {
            percent = 0;
            percentSlider.fillAmount = percent / 100;
            lb_Percent.text = $"{0}%";

        }
    
    }
    public bool CheckUnlock()
    {
        int playerLevel = IngameController.instance.GetPlayerLevel();

        if ((int)CardType == 0) return true;

        if (playerLevel > (int)CardType * 10) return true;
        return false;

    }


    public float PercentCalculator()
    {

        if (CardCount <= 0) return 0;
        int totalCardColor = 32;
        float percent = Mathf.Round(((float)CardCount / (float)totalCardColor) * 100) ;
        Debug.Log("percent" + percent + "card count"+ cardCount);
        return percent;
    }
}
