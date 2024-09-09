using System;
using UnityEngine;

public class DailyMainScreen : MonoBehaviour
{
    [SerializeField] private Animator anim;
    Action callback;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(DaiylRemainIcon), 1, 2f);
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void DailyRemainAnim(Action callback)
    {
        this.callback = callback;
        anim.Play("DailyPlay");
    }
    public void DailyClaimAnim(Action callback)
    {
        this.callback = callback;
        anim.Play("DailyHide");
    }
    public void DailyShow()
    {
        callback?.Invoke();
    }
    public void DailyHide()
    {
        callback?.Invoke();
    }
    public void DaiylRemainIcon()
    {
        if (anim == null) return;
        //if (DayTimeController.instance.isNewDay == true && DayTimeController.instance !=null)
        //{
        //    DailyRemainAnim(null);
        //}
        //else
        //{
        //    DailyClaimAnim(null);
        //}
    }
}
