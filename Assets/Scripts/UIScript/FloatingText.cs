using System;
using UnityEngine;

public class FloatingText :MonoBehaviour
{
    [SerializeField] private Animator floatingAnim;
    Action callback;
    private void OnEnable()
    {
        
    }
    public void ShowFloatingText()
    {
        floatingAnim.Play("FloatingAnim");
        //Debug.Log("play floating text");
    }
    public void ShowAnim()
    {
        callback?.Invoke();
    }
    public void DespawnGameObject(){
        gameObject.SetActive(false);
    }
    public void ActiveGameObject()
    {
        gameObject.SetActive(true);

    }
}
