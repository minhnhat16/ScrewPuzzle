using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LableTab : MonoBehaviour
{
    public CollectionLable type;
    public UnityEvent<CollectionLable> onChooseLable = new();
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }
}

public enum Lable
{
    // lable for mainscreen
    Home,
    Collection,
    Rate,
    Spin,
}

public enum CollectionLable
{
    // lable for collection
    BackGround,
    BoardColor,
    Screw,
}