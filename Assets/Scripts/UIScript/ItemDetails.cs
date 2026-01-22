using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetails : MonoBehaviour
{
    [SerializeField]
    private Image img;

    [SerializeField]
    private Text total_txt;


    public void Spawn(int total, Sprite sprite)
    {
        TextTotal(total);
        SetImg(sprite);
    }
    public void TextTotal(int total)
    {
        int i = total < 0 ? 0 : total;
        total_txt.text = "x" + total.ToString();
    }
    public void SetImg(Sprite sprite)
    {
        this.img.sprite = sprite;
    }
}
