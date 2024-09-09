using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardPicker : MonoBehaviour
{
    [SerializeField] private Button claimbtn;
    [SerializeField] private CardType cardType;
    [SerializeField] private CardPickerType pickerType;
    [SerializeField] private CardColorPallet color;
    [SerializeField] private Animator animator;
    [SerializeField] private Image cardImg;
    [SerializeField] private Action callback;
    public CardType CardType { get => cardType; set => cardType = value; }
    public CardPickerType PickerType { get => pickerType; set => pickerType = value; }
    public CardColorPallet Color { get => color; set => color = value; }
    public Button Claimbtn { get => claimbtn; set => claimbtn = value; }

    public UnityEvent<int> cardChose = new();
    // Start is called before the first frame update
    private void OnEnable()
    {
        Claimbtn.onClick.AddListener(OnButtonClicked);
        GetComponent<RectTransform>().localScale = Vector3.one;
    }

    private void OnDisable()
    {
        Claimbtn.onClick.RemoveAllListeners();
    }

    public void Init(CardPickerType pickerType, CardColorPallet color, CardType cardType)
    {
        this.pickerType = pickerType;
        this.color = color;
        this.cardType = cardType;
        if (Color == CardColorPallet.Empty)
        {
            gameObject.SetActive(false);
            return;
        }
        //Debug.Log($"COLOR {color}");
        cardImg.sprite = SpriteLibControl.Instance.GetCardSpriteByCategory(cardType, (int)color);
        if (!claimbtn.isActiveAndEnabled)
        {
            claimbtn.gameObject.SetActive(true);
            claimbtn.interactable = true;
        }
    }
    public void OnButtonClicked()
    {
        Claimbtn.interactable = false;
        switch (PickerType)
        {
            case CardPickerType.Premium:
                ZenSDK.instance.ShowVideoReward((isWatched) =>
                {
                    ClaimCardReward(isWatched);
                    //if (!isWatched) Debug.LogError("CAN'T COLLECT REWARD");
                });
                return;
            case CardPickerType.Free:
                ClaimCardReward(true);
                return;

        }
    }
    public void ClaimCardReward(bool isClaimed)
    {
        var type = IngameController.instance.CurrentCardType;
        if (isClaimed)
            DataAPIController.instance.SaveNewCardColor(Color, type, () =>
            {
            //play card claimed anim
            cardChose?.Invoke((int)color);
            //when done hide dialogs
            });
    }
    public void PlayClaimAnim(Action callback)
    {
        this.callback = callback;
        //Debug.Log("Play Claim Anim " + callback is null);
        animator.Play("ClaimingCard");
    }
    public void OnAnimDone()
    {
        //Debug.Log("ONANIMDONE");
        callback.Invoke();
    }
}
