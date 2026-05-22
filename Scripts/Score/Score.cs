using Godot;
using System;
using System.Linq;

public partial class Score : Node3D
{
	public static int TeamScore, OpponentScore;
	[Export] public RigidBody3D Ball;
	public static bool Scored_Wall, Scored_Catch;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TeamScore = 0;
		OpponentScore = 0;
		(Ball as Ball).OnBallThrow += ResetScoreBooleans;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CheckScore();
	}
	public void CheckScore() {
		if(Scored_Wall) return;
		if(GameManager.CWall) {
			if(GameManager.thrower == GameManager.ThrowerEnum.PLAYER || 
						GameManager.thrower == GameManager.ThrowerEnum.TEAM) {
				int Running = NpcController.NPCS.Where(n => n.TEAM == 1 && n.IsRunning).Count();
				TeamScore += Running;
			} else if(GameManager.thrower == GameManager.ThrowerEnum.OPPONENT) {
				int Running = NpcController.NPCS.Where(n => n.TEAM == 0 && n.IsRunning).Count();
				OpponentScore += Running;
				if(!PlayerManager.IsSafe) {
					OpponentScore++;
					PlayerManager.Reset = (PlayerManager.Reset) ? false : true;
				}
			}
			Scored_Wall = true;
			GameManager.CWall = false;
		}
	}
	public void CatchScore() {
		if(Scored_Catch) return;
		GameManager.Pitches = -1;
		// Use previous thrower (who threw the ball) and possession (who caught it)
		if(GameManager.previous_thrower == GameManager.ThrowerEnum.PLAYER || 
					GameManager.previous_thrower == GameManager.ThrowerEnum.TEAM) {
			if(GameManager.possession == GameManager.PossessionEnum.OPPONENT) {
				OpponentScore++;
				Scored_Catch = true;
			}
		} else if(GameManager.previous_thrower == GameManager.ThrowerEnum.OPPONENT) {
			if(GameManager.possession == GameManager.PossessionEnum.TEAM ||
						GameManager.possession == GameManager.PossessionEnum.PLAYER) {
				TeamScore++;
				Scored_Catch = true;
			}
		}
	}

	private void ResetScoreBooleans() {
		if(Scored_Catch || Scored_Wall) {
			Scored_Wall = false;
			Scored_Catch = false;
		}
	}
}
