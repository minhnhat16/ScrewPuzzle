using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class BaseDialog : MonoBehaviour
{
    public bool isInitDone = false;
    public DialogIndex dialogIndex;
    [SerializeField] internal BaseDialogAnimation baseDialogAnim;

    public BaseDialogAnimation BaseDialogAnim { get => baseDialogAnim; set => baseDialogAnim = value; }

    private void Awake()
    {
        baseDialogAnim = gameObject.GetComponentInChildren<BaseDialogAnimation>();
    }

    public IEnumerator Init()
    {
        isInitDone = false;
        OnInit(() =>
        {
            isInitDone = true;
            gameObject.SetActive(!isInitDone);
        });
        yield return new WaitUntil(()=> isInitDone);
    }

    public virtual void OnInit(Action callback = null) { callback?.Invoke(); }
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
