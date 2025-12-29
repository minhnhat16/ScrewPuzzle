using Mono.Cecil.Cil;
using System;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleView : BaseView
{
    [Header("Board")]
    [SerializeField] private PuzzleBoardUI boardView;

    [Header("Config")]
    [SerializeField] private PuzzleBoardRecord boardConfig;

    private PuzzleBoardRuntime runtime;
    [SerializeField]
    private Button returnBtn;

    public int IdPuzzle = 1;


    public Text lb_toolScrew;
    private PuzzleParam param;



    private void OnEnable()
    {
        returnBtn.onClick.AddListener(OnClickReturn);
    }



    private void OnDisable()
    {
        returnBtn.onClick.RemoveListener(OnClickReturn);
    }
    // =====================================================
    // SETUP
    // =====================================================
    public override void Setup(ViewParam param)
    {
        base.Setup(param);
        this.param = (PuzzleParam)param;
        lb_toolScrew.text = this.param.currentTool.ToString();
        LoadPuzzle(this.param.idPuzzle);
    }

    // =====================================================
    // LOAD PUZZLE
    // =====================================================
    private void LoadPuzzle(int id)
    {
        var boardConfig = ConfigFileManager.Instance
            .GetConfig<PuzzleConfig>()
            .GetRecordByKeySearch(id);

        if (boardConfig == null)
        {
            Debug.LogError("[PuzzleView] Missing board config with: " + id);
            return;
        }

        var pattern = ConfigExtensions
            .GetPartenBy(ConfigFileManager.Instance, boardConfig.parternId);

        if (pattern == null)
        {
            Debug.LogError("[PuzzleView] Missing pattern");
            return;
        }

        // 1️⃣ build runtime (STATE ONLY)
        runtime = new PuzzleBoardRuntime();

        // 2️⃣ init board view (orchestrator)
        boardView.Init(runtime);

        // 3️⃣ load pattern & register blocks
        boardView.LoadPattern(boardConfig, pattern);

        Debug.Log($"[PuzzleView] Loaded puzzle pattern {pattern.patternId}");
    }

    private void OnClickReturn()
    {
        ViewManager.Instance.SwitchView(ViewIndex.MainScreenView);
    }

}

public class PuzzleParam : ViewParam
{
    public int currentTool;
    public float progress;
    public float target;
    public int idPuzzle;
}