using UnityEngine;
using UnityEngine.UI;

public class LoadingView : BaseView
{
    public Slider loadingProgress;
    public Text loaddingText;

    private float dotTimer = 0;

    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);
    }

    public override void OnStartShowView()
    {   
        base.OnStartShowView();
        loadingProgress.value = 0;
    }

    private void Update()
    {
        UpdateLoadingProgress();
    }

    private void UpdateLoadingProgress()
    {
        // =====================
        //   TASK MANAGER PROGRESS
        // =====================

        float totalTasks = Mathf.Max(1, TaskManager.ins.TaskCount);
        float basicProgress = TaskManager.ins.TotalProgress;              
        float currentTaskProgress = TaskManager.ins.CurrentTaskProgress; 

        loadingProgress.value = Mathf.Lerp(0, totalTasks, totalTasks * 0.5f); ;

        // =====================
        //   LOADING TEXT ANIMATION
        // =====================

        dotTimer += Time.deltaTime;

        if (dotTimer < 0.5f)
            loaddingText.text = "Loading.";
        else if (dotTimer < 1.0f)
            loaddingText.text = "Loading..";
        else if (dotTimer < 1.5f)
            loaddingText.text = "Loading...";
        else
            dotTimer = 0;
    }
}
