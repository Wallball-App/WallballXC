using Godot;
using System;
using System.Linq;

public partial class Score : Node3D
{
	public static int TeamScore, OpponentScore;
	[Export] public RigidBody3D Ball;
	private bool cwall, cfloor;
	public bool Scored;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TeamScore = 0;
		OpponentScore = 0;
		Ball.BodyEntered += OnCollide;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CheckScore();
		if(Scored && !GameManager.HitWall) Scored = false;
	}
	public void CheckScore() {
		if(Scored) return;
		if(cwall) {
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
		}
		Scored = true;
	}
	public void CatchScore() {
		if(Scored) return;
		GameManager.Pitches = -1;
			if(GameManager.previous == GameManager.ThrowerEnum.PLAYER || 
						GameManager.previous == GameManager.ThrowerEnum.TEAM) {
				if(GameManager.possession == GameManager.PossessionEnum.OPPONENT) {
					OpponentScore++;
				}
			} else if(GameManager.previous == GameManager.ThrowerEnum.OPPONENT) {
				if(GameManager.possession == GameManager.PossessionEnum.TEAM ||
							GameManager.possession == GameManager.PossessionEnum.PLAYER) {
					TeamScore++;
				}
			}
		Scored = true;
	}
	public void OnCollide(Node node) {
		cwall = (node.Name == "Wall");
		cfloor = (node.Name == "Ground");
	}
}
