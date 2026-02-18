using Godot;
using System;

public partial class AnimationController : Node3D
{
	[Export] public AnimationPlayer Running, Throwing;
	[Export] public Node3D RunningSprite, ThrowingSprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Throwing.AnimationFinished += OnAnimationFinish;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void PlayRun() {
		ThrowingSprite.Visible = false;
		RunningSprite.Visible = true;
		Running.GetAnimation("Running/Running").LoopMode = Animation.LoopModeEnum.Linear;
		Running.Play("Running/Running");
	}
	public void PlayThrow() {
		RunningSprite.Visible = false;
		ThrowingSprite.Visible = true;
		Throwing.GetAnimation("Throwing/Throwing").LoopMode = Animation.LoopModeEnum.None;
		Throwing.Play("Throwing/Throwing");
	}
	private void OnAnimationFinish(StringName name) {
		if(name == "Throwing/Throwing") {
			PlayRun();
		}
	}

}
