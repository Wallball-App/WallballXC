using Godot;
using System;

public partial class Preview : Node3D
{
	Tween rotateTween;
	Tween movementTween;

	[Signal]
	public delegate void CutsceneEndedEventHandler();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Setup();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	private void Setup()
	{
		GlobalPosition += new Vector3(0.0f, 15.0f, 0.0f);
		GlobalRotationDegrees = new Vector3(-10.0f, -90.0f, 0.0f);

		rotateTween = GetTree().CreateTween();
		rotateTween.Finished += Movement;
		rotateTween.TweenProperty(this, "rotation_degrees:y", 90.0f, 10f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);
		rotateTween.Play();
	}
	private void Movement()
	{
		GlobalRotationDegrees = new Vector3(0.0f, 180.0f, 0.0f);
		GlobalPosition = new Vector3(-32.0f, 5.0f, 22.5f);
		
		movementTween = GetTree().CreateTween();
		
		movementTween.Finished += () => EmitSignal(SignalName.CutsceneEnded);
		movementTween.TweenProperty(this, "global_position:x", 32.0f, 5.0f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);
		movementTween.Play();
	}
	
}
