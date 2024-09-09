using UnityEngine;
using UnityEngine.UI;

public class CollectionItem : MonoBehaviour
{
    [SerializeField] private bool isLock;
    [SerializeField] private Image cardImg;
    [SerializeField] private GameObject lockImage;

    public bool IsLock { get => isLock; set => isLock = value; }
    public Image CardImg { get => cardImg; set => cardImg = value; }
    public GameObject LockImage { get => lockImage; set => lockImage = value; }
    public CollectionItem(Sprite cardImg, bool isLock = true)
    {
        this.CardImg.sprite = cardImg;
        this.IsLock = isLock;
        LockSprite(isLock); 
    }
    public void LockSprite(bool isLocked)
    {
        lockImage.gameObject.SetActive(isLocked);
    }
}
