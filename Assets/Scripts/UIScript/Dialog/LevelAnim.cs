using System;
using UnityEngine;

namespace UIScript.Dialog
{
    public class LevelAnim : BaseViewAnimation

    {
        public Animator animator;
        private Action _callback;

        public override void HideViewAnimation(Action callback)
        {
            this._callback = callback;
            //Debug.Log("RateHideAnim");
            animator.Play("LevelHideAnim");
        }

        public override void ShowViewAnimation(Action callback)
        {
            this._callback = callback;
            //Debug.Log("RateShowAnim");
            animator.Play("LevelShowAnim");
        }

        public void ShowAnim()
        {
            _callback?.Invoke();
        }

        public void HideAnim()
        {
            //Debug.Log("HideAnim");
            _callback?.Invoke();
        }
        public void Clear()
        {
            _callback?.Invoke();
        }
        public void RateCallBack()
        {
            _callback?.Invoke();
        }
    }
}
