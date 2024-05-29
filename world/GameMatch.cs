using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using wizardballz;
using wizardballz.world;
using Array = System.Array;

public partial class GameMatch : Node
{
    [Signal]
    public delegate void AllPlayersAvailableEventHandler();

    [Signal]
    public delegate void PlayStateChangedEventHandler(uint newPlayState);
        
    [Export] public GameField Field = null!;
    [Export] public Ball Ball = null!;

    [Export] public float CountdownDuration = 3;
        
    public IEnumerable<GamePlayer> Players => GetChildren().OfType<GamePlayer>();

    public Aabb BallBoundingBox;

    public float CountdownTimer { get; private set; }

    public MatchPlayState PlayState
    {
        get => _playState;
        set
        {
            var old = PlayState;
            _playState = value;
            if (PlayState != old) 
                EmitSignal(SignalName.PlayStateChanged, (uint)PlayState);
        }
    }

    private Vector3 BallSpawnGlobalPosition;
    
    private MatchPlayState _playState = MatchPlayState.Init;

    public GamePlayer PlayerFromClientId(ulong id)
    {
        return Players.Single(p => p.ClientId == id);
    }

    public override void _Ready()
    {
        BallSpawnGlobalPosition = Ball.GlobalPosition;

        Ball.Freeze = true;
        
        ChildEnteredTree += node =>
        {
            if (node is GamePlayer) {
                if (PlayState is MatchPlayState.Init or MatchPlayState.Countdown) {
                    node.CallDeferred(Node.MethodName.SetProcess, false);
                }
                
                // Ensure we don't go above max player count
                if (Players.Count() == WizardBallz.PlayerCount) {
                    EmitSignal(SignalName.AllPlayersAvailable);
                } else if (Players.Count() > WizardBallz.PlayerCount) {
                    throw new Exception("cannot add another player!");
                }
            }
        };
        
        SetState(MatchPlayState.Init);
    }

    public override void _Process(double delta)
    {
        for (var i = 0; i < Field.GoalZones.Length; i++) {
            if (Ball.Area.OverlapsArea(Field.GoalZones[i])) {
                GD.Print($"Goal for player {(i + 1) % 2}!");
                CallDeferred(nameof(OnBallEnteredGoal), (uint)i);
            }     
        }
        
        if (PlayState == MatchPlayState.Countdown && CountdownTimer > 0) {
            CountdownTimer -= (float)delta;
            if (CountdownTimer <= 0) {
                SetState(MatchPlayState.Active);
            }
        }
    }

    private void OnBallEnteredGoal(uint playerGoalIndex)
    {
        Players.ElementAt((int)playerGoalIndex).Score++;
        Ball.LinearVelocity = Vector3.Zero;
        Ball.AngularVelocity = Vector3.Zero;
        Ball.GlobalPosition = BallSpawnGlobalPosition;
    }

    public void SetState(MatchPlayState state)
    {
        PlayState = state;
        switch (PlayState) {
            case MatchPlayState.Init:
                Ball.Freeze = true;
                foreach (var child in this.GetChildrenOfType<GamePlayer>()) 
                    child.SetProcess(false);
                break;
            case MatchPlayState.Countdown:
                foreach (var child in this.GetChildrenOfType<GamePlayer>()) 
                    child.SetProcess(false);
                CountdownTimer = CountdownDuration;
                break;
            case MatchPlayState.Active:
                Ball.Freeze = false;
                foreach (var child in this.GetChildrenOfType<GamePlayer>()) 
                    child.SetProcess(true);
                break;
        }
    }

    public GamePlayer GetLocalPlayer()
    {
        return Players.Single(p => p.ClientId == (uint)Multiplayer.GetUniqueId());
    }

    public enum MatchPlayState
    {
        Init,
        Countdown,
        Active
    }

    public bool HasAllPlayers()
    {
        return Players.Count() == WizardBallz.PlayerCount;
    }
}