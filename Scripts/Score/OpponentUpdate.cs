using Godot;
using System;

public partial class OpponentUpdate : Label
{
	int PreviousScore;
	int CurrentScore;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = Score.OpponentScore.ToString();
		PreviousScore = CurrentScore;
		CurrentScore = Score.OpponentScore;
		if(PreviousScore != CurrentScore) {
			((GetParent() as PanelContainer).GetThemeStylebox("panel") as StyleBoxFlat).BgColor = new Color(0.5f,0.5f,0.5f,0.75f);
			CreateTween().TweenProperty(
				((GetParent() as PanelContainer).GetThemeStylebox("panel") as StyleBoxFlat),
				"bg_color", new Color(0f, 0f, 0f, 0.4f), 0.5f);
		}
	}
}
