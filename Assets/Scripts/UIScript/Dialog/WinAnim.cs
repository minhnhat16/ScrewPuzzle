using System;

namespace UIScript.Dialog
{
    public class WinAnim : BaseDialogAnimation
    {
        public override void Awake()
        {

            /*canvasGroup = gameObject.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }*/
        }

        public override void ShowDialogAnimation(Action callback)
        {
            /*canvasGroup.DOFade(1, 0.5f).OnComplete(() =>
            {
                callback();
            }).SetUpdate(true);*/

            callback();
        }

        public override void HideDialogAnimation(Action callback)
        {
            /*canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                callback();
            }).SetUpdate(true);*/

            callback();
        }

        public virtual void PlaySound() {
            SoundHelper.PlaySFX(SoundManager.SFX.Dialog_Appear);
        
        }
    }
}
