using Mono.Cecil.Cil;
using System;
using System.DataBase;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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

        runtime = new PuzzleBoardRuntime();
        boardView.Init(param.currentTool,runtime);
        boardView.LoadPattern(boardConfig, pattern);

        RegisterEventBoard();

        Debug.Log($"[PuzzleView] Loaded puzzle pattern {pattern.patternId}");
    }
    public void RegisterEventBoard()
    {
        boardView.updatePlayerScrew += UpdateCurrentToolAndParam;
        boardView.grandPrize += ShowReward;
    }

    private void ShowReward(int rewarId)
    {
        DialogManager.ins.ShowDialog(DialogIndex.GiftClaimDialog, new GiftParam()
        {
            rewards = ConfigExtensions.GetRewardConfig(ConfigFileManager.Instance, rewarId).Items
        }, null);
    }

    private void UpdateCurrentToolAndParam(int tools)
    {
        param.currentTool = tools;
        lb_toolScrew.text = tools.ToString();
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