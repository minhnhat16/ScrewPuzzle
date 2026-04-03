using Core.Match;
using Gameplay.StateMachine;
using Ingame;
using Managers;
using System.Collections;
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

        // Check if player has no tickets and no ads available
        long currentTicket = WalletManager.ins.Get(Currency.Ticket);
        bool isVideoRewardReady = AdsManager.instance.isVideoRewardReady();


        // Show lose dialog normally with revive options
        var loseParam = new LoseParam
        {
            isAdAvailable = isVideoRewardReady
        };
        _dialogService.ShowLoseDialog(loseParam);
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
                
                // ✅ Delay state transition to allow spawn animation to complete
                // This prevents TriggerQueueFull() from firing too early
                IngameController.ins.StartCoroutine(
                    DelayedTransition(GameplayState.Playing, 0.5f)
                );
            }
        };

        _dialogService.ShowReviveDialog(param, onDeclined: () =>
        {
            _dialogService.HideAllDialog();
            _stateMachine.TransitionTo(GameplayState.Lose);
        });
    }

    private IEnumerator DelayedTransition(GameplayState targetState, float delay)
    {
        yield return new WaitForSeconds(delay);
        _stateMachine.TransitionTo(targetState);
    }

    #endregion

    #region RESTART

    public void RestartLevel(int levelId)
    {
        _dialogService.HideAllDialog();

        if (_levelManager is LevelManager concreteLevelManager)
        {
            concreteLevelManager.ReLoadLevel(() =>
            {
                _stateMachine.TransitionTo(GameplayState.Playing);
            });
            return;
        }

        _arrayScrew.Clear();
        _boxQueue.Reset();
        _levelManager.Dispose();
        _levelManager.LoadLevel(levelId, () =>
        {
            _stateMachine.TransitionTo(GameplayState.Playing);
        });
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
        _player.UnlockInput();
        var param = new SettingParam
        {
            isMainScreen = ViewManager.Instance.currentView is MainScreenView,
            totalGold = WalletManager.ins.Get(Currency.Gold),
            totalTicket = WalletManager.ins.Get(Currency.Ticket),
            title = "PAUSE",
            music_enable = SoundHelper.IsMusicEnabled(),
            sfx_enable = SoundHelper.IsSFXEnabled(),
            onResumed = () =>
            {
                _dialogService.HideAllDialog();
                _player.UnlockInput();
                _stateMachine.TransitionTo(GameplayState.Playing);
            }
        };

        // Pass null as callback to ShowPause - the dialog will handle the onResumed callback internally
        _dialogService.ShowPause(param, onResumed: null);
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
