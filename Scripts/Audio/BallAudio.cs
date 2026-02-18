using Godot;
using System;

public partial class BallAudio : AudioStreamPlayer3D
{
	private float LastPlay;
	private float Frames;
	private float Timeout = 0.5f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Frames = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
