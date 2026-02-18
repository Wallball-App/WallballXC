using Godot;
using System;

public partial class PlayerManager : Node3D
{

	[Export] public StaticBody3D Wall, Ground;
	[Export] public RigidBody3D Ball;
	[Export] public Node3D SafePoint;
	[Export] public CharacterBody3D Player;
	
	public static int Frames;
	public static bool IsSafe;
	
	public static float wallc, floorc;
	private bool TouchingBall;
	private Ball BallControl;
	public static bool Reset;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Frames = 0;
		Ball.BodyEntered += OnBallCollide;
		BallControl = GetNode<Ball>("%Ball");
		
		wallc = -1;
		floorc = -1;
		IsSafe = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateVariables();
		CheckSafe();
		//GD.Print(Player.GlobalPosition);
		Frames++;
	}
	public void OnBallCollide(Node node) {
		if(node.Name == "Wall") {
			if(wallc == -1) wallc = Frames;
		} else if(node.Name == "Ground") {
			if(floorc == -1) floorc = Frames; //CFBW
		}
		if(node.Name == "Player") {
			TouchingBall = true;
		} else {
			TouchingBall = false;
		}
	}
	public bool CFBW() {
		bool result = (wallc == -1 && floorc > -1) || (floorc < wallc && floorc != -1);
		if(GameManager.thrower != GameManager.ThrowerEnum.PLAYER) result = false;
		return result;
	}
	public bool OnPlayerWallCollide() {
		if(Player.GlobalPosition.Z > SafePoint.GlobalPosition.Z) return true;
		return false;
	}
	public void CheckSafe() {
		if(IsSafe) {
			if(CFBW() || TouchingBall) {
				IsSafe = false;
				if(GameManager.possession == GameManager.PossessionEnum.PLAYER) {
					BallControl.Throw(GameManager.ThrowerEnum.PLAYER, Vector3.Down, 1);
				}
			}
		} else {
			if(OnPlayerWallCollide() || Reset) {
				IsSafe = true;
				if(CFBW()) floorc = Frames;
			}
		}
		//GD.Print(IsSafe + ", Wall: " + wallc + "Floor: " + floorc);
		//GD.Print(Ball.GlobalPosition);
	}
	private void UpdateVariables() {
		if(GameManager.possession != GameManager.PossessionEnum.NONE) {
			floorc = -1;
			wallc = -1;
		} 
	}
}
