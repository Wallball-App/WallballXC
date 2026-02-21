using Godot;
using System;

public partial class GameManager : Node3D
{
	public enum ThrowerEnum {PLAYER, TEAM, OPPONENT, NONE, RUNNING};
	public static ThrowerEnum thrower;
	public static ThrowerEnum previous;
	
	public enum PossessionEnum {PLAYER, TEAM, OPPONENT, NONE};
	public static PossessionEnum possession;
	
	[Export] public StaticBody3D Wall, Ground;
	public static RigidBody3D Ball;
	public AudioStreamPlayer3D BallAudio;
	public static bool CWall, CFloor;
	public static int Pitches;
	public static bool HitWall;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Ball = GetNode<RigidBody3D>("%Ball");
		Ball.BodyEntered += OnBallCollide;
		BallAudio = Ball.GetNode<AudioStreamPlayer3D>("BallAudio");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(CWall) {
			Pitches = 0;
			HitWall = true;
		} if(CFloor) {
			Pitches++;
		}
		if(GameManager.thrower == GameManager.ThrowerEnum.NONE) {
			HitWall = false;
		}
		//GD.Print(Ball.GlobalPosition.Y);
		//GD.Print("Thrower: " + thrower + ", Previous: " + previous);
	}
	public void OnBallCollide(Node node) {
		CWall = (node.Name == "Wall");
		CFloor = (node.Name == "Ground");
		if(CWall) {
			float lvel = Ball.LinearVelocity.Length();
			BallAudio.VolumeDb = Mathf.Remap(lvel, 0, 5, -60, 0);
			BallAudio.PitchScale = 1;
			BallAudio.Play();
		}
		if(CFloor) {
			float lvel = Ball.LinearVelocity.Length();
			BallAudio.VolumeDb = Mathf.Remap(lvel, 0, 5, -60, 0);
			BallAudio.PitchScale = 2;
			BallAudio.Play();
		}
	}
}
