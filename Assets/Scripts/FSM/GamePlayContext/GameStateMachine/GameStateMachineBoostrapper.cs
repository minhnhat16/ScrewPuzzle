using Gameplay.StateMachine;
using UnityEngine;

public class GameStateMachineBootstrapper : MonoBehaviour, IGameStateMachine
{
    // ─────────────────────────────────────────
    // IGameStateMachine implementation
    // ─────────────────────────────────────────

    public GameplayState Current => _machine != null ? _machine.Current : GameplayState.Idle;

    public event System.Action<GameplayState, GameplayState> OnStateChanged
    {
        add { if (_machine != null) _machine.OnStateChanged += value; else _pendingSubscribers += value; }
        remove { if (_machine != null) _machine.OnStateChanged -= value; }
    }

    public bool TransitionTo(GameplayState next) => _machine.TransitionTo(next);
    public bool IsPlaying() => _machine != null && _machine.IsPlaying();
    public bool IsIn(GameplayState state) => _machine != null && _machine.IsIn(state);
    public bool CanTransition(GameplayState from, GameplayState to) => _machine != null && _machine.CanTransition(from, to);

    // ─────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────

    private GameStateMachine _machine;

    // Buffer subscribers đăng ký trước Awake
    private System.Action<GameplayState, GameplayState> _pendingSubscribers;

    private void Awake()
    {
        PsbSlidingWindowLoader.ins.Reset();
        InitializeMachine();
    }

    public void ResetMachine()
    {
        InitializeMachine();
    }

    private void InitializeMachine()
    {
        _machine = new GameStateMachine();

        _machine.RegisterState(new IdleState());
        _machine.RegisterState(new LoadingState());
        _machine.RegisterState(new PlayingState());
        _machine.RegisterState(new PausedState());
        _machine.RegisterState(new WinState());
        _machine.RegisterState(new LoseState());
        _machine.RegisterState(new RevivePromptState());
        _machine.RegisterState(new ItemUsingState());

        // Flush pending subscribers đăng ký trước Awake
        if (_pendingSubscribers != null)
        {
            _machine.OnStateChanged += _pendingSubscribers;
            _pendingSubscribers = null;
        }

        _machine.Start(GameplayState.Idle);

        Debug.Log("[GameStateMachineBootstrapper] Machine initialized → Idle");
    }

    private void Update()
    {
        _machine?.Tick(Time.deltaTime);
    }
}