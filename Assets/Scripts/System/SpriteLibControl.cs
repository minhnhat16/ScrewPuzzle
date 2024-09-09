using System.Collections.Generic;
using UnityEngine;

public class SpriteLibControl : MonoBehaviour
{
    public static SpriteLibControl Instance;

    [SerializeField]
    private List<Sprite> _sprite;
    [SerializeField]
    private List<Sprite> _BG;
    [SerializeField]
    private List<Sprite> defaultCards;
    [SerializeField]
    private List<Sprite> lozengeCards;
    [SerializeField]
    private List<Sprite> legoCards;
    [SerializeField]
    private List<Sprite> cassetteCard;
    [SerializeField]
    private List<Sprite> mailCards;
    [SerializeField]
    private List<Sprite> discCards;
    [SerializeField]
    private List<Sprite> shirtCards;
    [SerializeField]
    private List<Sprite> cheeseCards;
    [SerializeField]
    private List<Sprite> giftCards;

    readonly private Dictionary<string, Sprite> spriteDict = new();

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        foreach (var sprite in _sprite)
        {
            spriteDict.Add(sprite.name, sprite);
        }
    }

    public Sprite GetSpriteByName(string name)
    {
        //Debug.Log($"GetSpriteByName{name}");
        if (spriteDict.ContainsKey(name)) return spriteDict[name];
        else return null;
    }
    public Sprite LoadBGSprite(CardType type)
    {
        int intType = (int)type;
        return _BG[intType] is null ? null : _BG[intType];
    }
    public Sprite GetCardSpriteByCategory(CardType type, int idSprite)
    {
        switch (type)
        {
            case CardType.Default: return defaultCards[idSprite];
            case CardType.Lozenge: return lozengeCards[idSprite];
            case CardType.Lego: return legoCards[idSprite];
            case CardType.Cassette: return cassetteCard[idSprite];
            case CardType.Mail: return mailCards[idSprite];
            case CardType.Dics: return discCards[idSprite];
            case CardType.Shirt: return shirtCards[idSprite];
            case CardType.Cheese: return cheeseCards[idSprite];
            case CardType.Gift: return giftCards[idSprite];
            default:  return defaultCards[idSprite];
        }
    }
}
