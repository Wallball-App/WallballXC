using Godot;
using System;

public partial class GameManager : Node3D
{
	public enum ThrowerEnum {PLAYER, TEAM, OPPONENT, NONE /*, RUNNING*/};
	public static ThrowerEnum thrower;
	
	public enum PossessionEnum {PLAYER, TEAM, OPPONENT, NONE};
	public static PossessionEnum possession;
	public static PossessionEnum previous_pos;
	public static ThrowerEnum previous_thrower;

	public enum CameraPerspectiveEnum {FIRST_PERSON, THIRD_PERSON};
	public static CameraPerspectiveEnum perspective;
	
	[Export] public StaticBody3D Wall, Ground;
	public static RigidBody3D Ball;
	public static bool CWall, CFloor;
	public static int Pitches;
	public static bool HitWall, HitFloor;
	public static int FRAMES;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Ball = GetNode<RigidBody3D>("%Ball");
		FRAMES = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FRAMES++;
	}
}
