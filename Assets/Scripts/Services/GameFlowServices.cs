using Core.Match;
using Ingame;
using UnityEngine;

public class GameFlowService : IGameFlowService
{
    private readonly IContainerQueue _boxQueue;
    private readonly ITempQueue _arrayScrew;
    private readonly ILevelManager _levelManager;
    private readonly IDialogService _dialogService;
    private readonly IPlayer _player;

    public GameFlowService(
        IContainerQueue containerQueue,
        ITempQueue arrayScrew,
        ILevelManager levelManager,
        IDialogService dialogService,
        IPlayer player)
    {
        _boxQueue = containerQueue;
        _arrayScrew = arrayScrew;
        _levelManager = levelManager;
        _dialogService = dialogService;
        _player = player;
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

        if (_boxQueue.ActiveCount >= 4)
        {
            _dialogService.ShowLoseDialog();
            return;
        }

        _dialogService.ShowReviveDialog();
    }

    public void HandleRevive()
    {
        _player.UnlockInput();
        _boxQueue.UnlockNext();
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
        _levelManager.ResetLevel();
        _dialogService.ReturnToMainMenu();
    }

    #endregion

    #region PAUSE
    public void PauseGame()
    {
        _player.LockInput();
        _dialogService.ShowPause();

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