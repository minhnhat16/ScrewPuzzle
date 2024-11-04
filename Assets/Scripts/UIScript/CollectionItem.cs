using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollectionItem : MonoBehaviour
{
    [SerializeField] private int index;

    [SerializeField] private bool isLock;
    [SerializeField] private Image img;
    [SerializeField] private GameObject lockImage;
    [SerializeField] private TogglePro toggle;
    [SerializeField] private Image tickIcn;

    public bool IsLock { get => isLock; set => isLock = value; }
    public Image Img { get => img; set => img = value; }
    public GameObject LockImage { get => lockImage; set => lockImage = value; }
    public TogglePro Toggle { get => toggle; set => toggle = value; }

    public UnityEvent<bool> toggleClicked = new();
    public void OnEnable()
    {
        toggle.checkIcon = tickIcn;
        
    }

    public void OnDisable()
    {
        //toggleClicked.RemoveListener(OnToggleValueChanged);
    }

    public void Start()
    {
       
       /* toggleClicked.AddListener((isClicked)=> {
            OnToggleValueChanged(isClicked);

        });*/

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
        Debug.LogWarning("Toggle add at index: " + index);
        if (isOn)
        {
            Debug.Log("Toggle clicked at index: " + index);
            // You can also handle other actions when a toggle is selected
        }
        //Debug.Log("Toggle clicked at index: " + index);
    }
    public void Init(int index)
    {
        this.index = index;
        toggleClicked.AddListener(OnToggleValueChanged);
        toggleClicked = toggle.onValueChanged;
        toggleClicked.AddListener(OnToggleValueChanged);
        Debug.Log("Toggle check listioner " + toggleClicked.GetPersistentEventCount());
        //throw new NotImplementedException();
    }
    internal void Init(int index, Image img)
    {
        this.index = index;
        this.img = img;
        toggleClicked.AddListener(OnToggleValueChanged);
        //throw new NotImplementedException();
    }
}
