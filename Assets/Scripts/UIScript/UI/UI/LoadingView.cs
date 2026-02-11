using UnityEngine;
using UnityEngine.UI;
public class LoadingView : BaseView
{
    public Slider loadingProgress;
    public Text loaddingText;

    [Header("Progress smoothing")]
    [SerializeField]
    private float progressSmoothSpeed = 0.8f; // units per second
    private float displayedProgress = 0f;

    private float dotTimer = 0;

    public void OnDisable()
    {
        // Don't auto-reset here. Keep progress visible if view is re-enabled unexpectedly.
    }

    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);
        ResetProgress();
    }

    public  void ShowViewAnimation()
    {
        base.ShowViewAnimation(() => { Debug.Log("Show loading anim"); });
    }

    public override void OnStartShowView()
    {
        base.OnStartShowView();
        Debug.Log("ON start show view  loading ");
        // Do not reset displayedProgress here either.
    }

    /// <summary>
    /// Call this explicitly when you start a new loading sequence.
    /// </summary>
    public void ResetProgress()
    {
        displayedProgress = 0f;
        if (loadingProgress != null)
            loadingProgress.value = 0f;
        dotTimer = 0f;
        Debug.Log("[LoadingView] ResetProgress()");
    }

    private void Update()
    {
        UpdateLoadingProgress();
    }

    private void UpdateLoadingProgress()
    {

        if (TaskManager.ins == null)
            return;

        // =====================
        //   TASK MANAGER PROGRESS
        // =====================

        float totalTasks = Mathf.Max(1, TaskManager.ins.TaskCount);
        float basicProgress = TaskManager.ins.TotalProgress;
        float currentTaskProgress = TaskManager.ins.CurrentTaskProgress;

        // Overall progress target (normalized 0..1)
        float target = Mathf.Clamp01(basicProgress + currentTaskProgress / totalTasks);

        // Prevent progress from going backwards while loading
        target = Mathf.Max(target, displayedProgress);

        // Smoothly move displayedProgress towards the target
        displayedProgress = Mathf.MoveTowards(displayedProgress, target, progressSmoothSpeed * Time.deltaTime);

        // snap small differences
        if (Mathf.Abs(displayedProgress - target) < 0.0005f)
            displayedProgress = target;

        if (loadingProgress != null)
            loadingProgress.value = displayedProgress;

#if UNITY_EDITOR
       //Debug.Log($"Loading Progress target={target * 100f:0.0}% shown={displayedProgress * 100f:0.0}%");
#endif

        // =====================
        //   LOADING TEXT ANIMATION
        // =====================

        dotTimer += Time.deltaTime;

        if (loaddingText != null)
        {
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
}