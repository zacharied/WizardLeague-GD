using Godot;
using Godot.Collections;
using wizardballz;
using wizardballz.world;
using Array = System.Array;

public partial class GameMatch : Node
{
    [Export] public GameField Field = null!;
    [Export] public Ball Ball = null!;
        
    public MatchPlayState PlayState = MatchPlayState.Init;
    public GamePlayer[] Players = new GamePlayer[WizardBallz.PlayerCount];

    private Vector3 BallSpawnGlobalPosition;

    public override void _Ready()
    {
        BallSpawnGlobalPosition = Ball.GlobalPosition;
        
        Ball.Area.AreaEntered += area =>
        {
            var i = Array.IndexOf(Field.GoalZones, area);
            if (i >= 0) {
                GD.Print($"Goal for player {i}!");
                CallDeferred(nameof(OnBallEnteredGoal), (uint)i);
            }
        };
    }

    private void OnBallEnteredGoal(uint playerGoalIndex)
    {
        Players[playerGoalIndex].Score++;
        Ball.LinearVelocity = Vector3.Zero;
        Ball.AngularVelocity = Vector3.Zero;
        Ball.GlobalPosition = BallSpawnGlobalPosition;
    }

    public enum MatchPlayState
    {
        Init,
        Countdown,
        Active
    }
}