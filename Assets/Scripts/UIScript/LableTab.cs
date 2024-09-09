using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LableTab : MonoBehaviour
{
    public Lable type;
    public Button chooseButton;
   [SerializeField] private Animator animator;
    public UnityEvent<Lable> onChooseLable = new();
    private void OnEnable()
    {
        chooseButton.onClick.AddListener(OnButtonClicked);
    }
    private void OnDisable()
    {
        chooseButton.onClick.RemoveListener(OnButtonClicked);

    }
    public void OnButtonClicked()
    {
        animator.Play("LableChoose");
        chooseButton.interactable = false;
        //Debug.Log($"on button clicked {type}");
        onChooseLable?.Invoke(type); 
    }
    public void OnButtonUnchose()
    {
        animator.Play("LableItemHide_idle");
        chooseButton.interactable = true;
        //Debug.Log($"on button unchoose {type}");
    }
}
