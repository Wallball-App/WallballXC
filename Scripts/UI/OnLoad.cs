using Godot;
using System;
using System.Threading.Tasks;

public partial class OnLoad : ColorRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Color = new Color(0, 0, 0, 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	public async Task FadeIn() {
		this.Color = new Color(0,0,0,1);
		GetTree().CreateTween().TweenProperty(this, "color", new Color(0, 0, 0, 0), 1.0f);
	}
	public async Task FadeOut() {
		Tween Fade = GetTree().CreateTween();
		Fade.TweenProperty(this, "color", new Color(0, 0, 0, 1), 1.0f);
		await ToSignal(Fade, Tween.SignalName.Finished);
		this.Color = new Color(0,0,0,0);
	}
}
