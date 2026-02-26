using Ingame;
using UnityEngine;

public class GameFlowService : IGameFlowService
{
    private readonly IBoxQueue _boxQueue;
    private readonly IArrayScrew _arrayScrew;
    private readonly ILevelManager _levelManager;
    private readonly IDialogService _dialogService;
    private readonly IPlayer _player;

    public GameFlowService(
        IBoxQueue boxQueue,
        IArrayScrew arrayScrew,
        ILevelManager levelManager,
        IDialogService dialogService,
        IPlayer player)
    {
        _boxQueue = boxQueue;
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

        _dialogService.ShowWinDialog(_levelManager.CurrentLevelId);
    }

    #endregion

    #region GAME OVER

    public void HandleGameOver()
    {
        _player.LockInput();

        if (_boxQueue.ActiveBoxCount >= 4)
        {
            _dialogService.ShowLoseDialog();
            return;
        }

        _dialogService.ShowReviveDialog();
    }

    public void HandleRevive()
    {
        _player.UnlockInput();
        _boxQueue.UnlockNextBox();
    }

    #endregion

    #region RESTART

    public void RestartLevel(int levelId)
    {
        _arrayScrew.Clear();
        _boxQueue.ResetQueue();
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