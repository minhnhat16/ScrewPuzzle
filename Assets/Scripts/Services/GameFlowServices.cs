using Core.Match;
using Gameplay.StateMachine;
using Ingame;
using UIScript.UI.UI;
using UnityEngine;

public class GameFlowService : IGameFlowService
{
    private readonly IContainerQueue _boxQueue;
    private readonly ITempQueue _arrayScrew;
    private readonly ILevelManager _levelManager;
    private readonly IDialogService _dialogService;
    private readonly IPlayer _player;
    private readonly IGameStateMachine _stateMachine;

    public GameFlowService(
        IContainerQueue containerQueue,
        ITempQueue arrayScrew,
        ILevelManager levelManager,
        IDialogService dialogService,
        IPlayer player,
        IGameStateMachine stateMachine)
    {
        _boxQueue = containerQueue;
        _arrayScrew = arrayScrew;
        _levelManager = levelManager;
        _dialogService = dialogService;
        _player = player;
        _stateMachine = stateMachine;
    }

    #region LEVEL COMPLETE

    public void CompleteLevel()
    {
        HandleGameWin();
    }

    public void HandleGameWin()
    {
        _player.LockInput();
        _dialogService.ShowWinDialog(new());
    }

    #endregion

    #region GAME OVER

    public void HandleGameOver()
    {
        _player.LockInput();
        _dialogService.ShowLoseDialog();
    }

    public void HandleRevive()
    {
        _player.LockInput();

        var param = new ReviveParam
        {
            isRevive = true,
            totalGold = WalletManager.ins.Get(Currency.Gold),
            currentTicket = WalletManager.ins.Get(Currency.Ticket),

            // Player chấp nhận revive → unlock box, về Playing
            onWatchAccepted = () =>
            {
                _dialogService.HideAllDialog();
                _player.UnlockInput();
                _boxQueue.UnlockNext();
                _stateMachine.TransitionTo(GameplayState.Playing);
            }
        };

        // onDeclined: player từ chối → về Lose → HandleGameOver sẽ show LoseDialog
        _dialogService.ShowReviveDialog(param, onDeclined: () =>
        {
            _dialogService.HideAllDialog();
            _stateMachine.TransitionTo(GameplayState.Lose);
        });
    }

    #endregion

    #region RESTART

    public void RestartLevel(int levelId)
    {
        _arrayScrew.Clear();
        _boxQueue.Reset();
        _levelManager.LoadLevel(levelId);
    }

    #endregion

    #region RETURN MENU

    public void HandleReturnToMenu()
    {
        _levelManager.Dispose();
        _dialogService.ReturnToMainMenu();
    }

    #endregion

    #region PAUSE

    public void PauseGame()
    {
        _player.LockInput();

        var param = new SettingParam
        {
            isMainScreen = ViewManager.Instance.currentView is MainScreenView,
            totalGold = WalletManager.ins.Get(Currency.Gold),
            totalTicket = WalletManager.ins.Get(Currency.Ticket),
            title = "PAUSE",
            music_enable = SoundHelper.IsMusicEnabled(),
            sfx_enable = SoundHelper.IsSFXEnabled(),
        };

        // onResumed: player bấm close/resume → unlock input, về Playing
        _dialogService.ShowPause(param, onResumed: () =>
        {
            _dialogService.HideAllDialog();
            _player.UnlockInput();
            _stateMachine.TransitionTo(GameplayState.Playing);
        });
    }

    #endregion

    #region RESUME

    public void ResumeGame()
    {
        _player.UnlockInput();
        _dialogService.HideAllDialog();
    }

    #endregion
}