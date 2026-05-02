using Godot;
using System;

public partial class Weather : Node3D
{
	public static Node Sky;
	public static Node SkyDome;
	public static Node TimeOfDay;

	public float CurrentTime;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Sky = GetNode<Node>("%Sky3D");
		SkyDome = Sky.GetNode<Node>("SkyDome");
		TimeOfDay = Sky.GetNode<Node>("TimeOfDay");

		//SetTime(19.2f);
		//SetClouds(1.0f, 1.0f, 8.0f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void SetTime(float time)
	{
		CurrentTime = time;
		Sky.Set("current_time", time);
	}
	private void SetClouds(float cirrus, float cumulus, float absorption)
	{
		SkyDome.Set("cirrus_coverage", cirrus);
		SkyDome.Set("cumulus_coverage", cumulus);

		SkyDome.Set("cirrus_absorption", absorption);
		SkyDome.Set("cumulus_absorption", absorption);
	}
}
