
public class LastDailyItem : DailyItem
{
    public void DebugButton()
    {
        //Debug.Log("On Click Daily Item");
    }
    public override void SwitchType(IEDailyType type)
    {
        currentType = type;
        daily_btn.enabled = true;
        switch (type)
        {
            case IEDailyType.Available:
                SetCanBeClaim();
                daily_btn.enabled = true;
                tickImg.gameObject.SetActive(false);
                onRewardRemain?.Invoke(true);
                break;
            case IEDailyType.Unavailable:
                SetCantClaim();

                break;
            case IEDailyType.Claimed:
                SetClaimed();
                daily_btn.enabled = false;
                Amount_lb.gameObject.SetActive(false);

                break;
            default:
                break;
        }
    }
    public override void OnClickDailyItem()
    {
        //Debug.Log("On Click Daily Item");
        if (currentType == IEDailyType.Available)
        {
            //var parent = DialogManager.Instance.dicDialog[DialogIndex.DailyRewardDialog].GetComponent<DailyRewardDialog>();
            //   parent.dailyGrid.currentDaily = this;
            onClickDailyItem?.Invoke(true);
        }
        else
        {
            onClickDailyItem?.Invoke(false);
        }
    }
}
