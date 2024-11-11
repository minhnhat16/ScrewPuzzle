using System;
using System.DataBase;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollectionItem : MonoBehaviour
{
    [SerializeField] private int index;

    [SerializeField] private bool isLock;
    [SerializeField] CollectionLable type;
    [SerializeField] private Image img;
    [SerializeField] private GameObject lockImage;
    [SerializeField] private TogglePro toggle;
    [SerializeField] private Graphic tickIcn;

    public bool IsLock { get => isLock; set => isLock = value; }
    public Image Img { get => img; set => img = value; }
    public GameObject LockImage { get => lockImage; set => lockImage = value; }
    public TogglePro Toggle { get => toggle; set => toggle = value; }
    public CollectionLable Type { get => type; set => type = value; }

    public UnityEvent<bool> toggleClicked = new();
    public void OnEnable()
    {
        toggleClicked = toggle.onValueChanged;
        toggleClicked.AddListener(OnToggleValueChanged);
    }

    public void OnDisable()
    {
        toggleClicked.RemoveListener(OnToggleValueChanged);
    }
    public void Awake()
    {
       toggle.checkIcon = tickIcn;
    }
    public CollectionItem(Sprite cardImg, bool isLock = true)
    {
        this.Img.sprite = cardImg;
        this.IsLock = isLock;
        LockSprite(isLock); 
    }
    public void LockSprite(bool isLocked)
    {
        lockImage.gameObject.SetActive(isLocked);
    }
    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            OnChangeSkin(Type);
            // You can also handle other actions when a toggle is selected
        }
        //Debug.Log("Toggle clicked at index: " + index);
    }

    private void OnChangeSkin(CollectionLable type, Action callback = null)
    {
        switch (type)
        {
            case CollectionLable.BackGround:
                BackGroundData newDataBG = new();
                newDataBG.name = img.sprite.name;
                newDataBG.isUnlocked = true;
                DataAPIController.instance.SetCurrentBackGroundData(newDataBG, callback);
                break;
            case CollectionLable.BoardColor:
                BoardColorData newDataBoarColor = new();
                newDataBoarColor.name = img.sprite.name;
                newDataBoarColor.isUnlocked = true;
                DataAPIController.instance.SetCurrentBoardData(newDataBoarColor, callback);
                break;

            case CollectionLable.Screw:
                ScrewSkinData newDataScrew = new();
                newDataScrew.name = img.sprite.name;
                newDataScrew.isUnlocked = true;
                DataAPIController.instance.SetCurrentScrewData(newDataScrew, callback);
                break;
        }
    }
    public void Init(int index,Sprite img, bool toggleEnable, CollectionLable type)
    {
        this.index = index;
        this.img.sprite = img;
        this.type = type;
        toggle.isOn = toggleEnable;
        //throw new NotImplementedException();
    }
}
