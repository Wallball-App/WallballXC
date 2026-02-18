using Godot;
using System;

public partial class SafeText : Label
{
	private bool pc, cc;
	public static bool Safe;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cc = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		pc = cc;
		cc = !PlayerManager.IsSafe;
		if(pc == false && cc == false) {
			Visible = false;
		} else if(pc == false && cc == true) {
			Visible = true;
		}
		Safe = !Visible;
	}
}
