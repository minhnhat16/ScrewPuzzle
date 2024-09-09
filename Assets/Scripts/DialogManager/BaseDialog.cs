using System;
using UnityEngine;
using UnityEngine.UI;
public class BaseDialog : MonoBehaviour
{
    public DialogIndex dialogIndex;
    [SerializeField]private BaseDialogAnimation baseDialogAnim;

    private void Awake()
    {
        baseDialogAnim = gameObject.GetComponentInChildren<BaseDialogAnimation>();
    }

    public void Init()
    {
        OnInit();
        gameObject.SetActive(false);
    }

    public virtual void OnInit() { }

    public virtual void Setup(DialogParam dialogParam) { }

    public void ShowDialogAnimation(Action callback)
    {
        baseDialogAnim.ShowDialogAnimation(() =>
        {
            //Debug.Log("Show Dialog anim");
            OnStartShowDialog();
            callback?.Invoke();
            OnEndShowDialog();
        });
    }

    public void HideDialogAnimation(Action callback)
    {
        baseDialogAnim.HideDialogAnimation(() =>
        {
            //Debug.Log("Hide dialog anim");
            OnStartHideDialog();
            callback?.Invoke();
            OnEndHideDialog();
        });
    }

    public virtual void OnStartShowDialog() { }

    public virtual void OnEndShowDialog() { }

    public virtual void OnStartHideDialog() { }

    public virtual void OnEndHideDialog() { }

    public virtual void ShowDialog() { }

    public virtual void HideDialog() { }
}
