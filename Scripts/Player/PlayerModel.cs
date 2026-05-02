using Godot;
using System;

public partial class PlayerModel : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.GlobalRotation = new Vector3(0.0f, PlayerInput.CamRotation.Y - (float)Math.PI/2.0f, 0.0f);
	}
}
