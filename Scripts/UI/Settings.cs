using Godot;
using System;

public partial class Settings : Button
{
	private CenterContainer Menu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Toggled += OnToggle;
		Menu = GetNode<CenterContainer>("%MenuCenter");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void OnToggle(bool toggled) {
		Menu.Visible = toggled;
	}
}
