
public class LastDailyItem : DailyItem
{
    public void DebugButton()
    {
        //Debug.Log("On Click Daily Item");
    }
    public override void SwitchType(DailyType type)
    {
        currentType = type;
        daily_btn.enabled = true;
        switch (type)
        {
            case DailyType.Available:
                SetCanBeClaim();
                daily_btn.enabled = true;
                tickImg.gameObject.SetActive(false);
                onRewardRemain?.Invoke(true);
                break;
            case DailyType.Unavailable:
                SetCantClaim();

                break;
            case DailyType.Claimed:
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
        if (currentType == DailyType.Available)
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
