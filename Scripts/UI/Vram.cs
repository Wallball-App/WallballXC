using Godot;
using System;

public partial class Vram : Label
{
	private float Megabyte = 1048576.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		double mb = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed)/Megabyte;
		float final = (float) Math.Round(mb, 3);
		Text = "Estimated VRAM Usage: " + final + " MB";
	}
}
