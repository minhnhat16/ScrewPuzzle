using Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BtnColor : MonoBehaviour
{

    private Text txtCount;
    private ColorEnum color;
    public UnityEvent<int> totalChange;


    private void OnEnable()
    {
        totalChange.AddListener(SetColorTotal);
    }
    private void OnDisable()
    {
        totalChange.RemoveListener(SetColorTotal);
    }
    private void Awake()
    {
        txtCount = GetComponentInChildren<Text>();
    }
    public ColorEnum Color { get => color; set => color = value; }

    public void SetColorTotal(int value)
    {
        txtCount.text = value.ToString();
    }
}
