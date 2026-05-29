using Godot;
using GodotCookies;
using System;

public partial class Weather : Node3D
{
	public static Node Sky;
	public static Node SkyDome;
	public static Node TimeOfDay;

    private float Time;
	private int CloudPreset;
	public static float CurrentTime;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Sky = GetNode<Node>("%Sky3D");
		SkyDome = Sky.GetNode<Node>("SkyDome");
		TimeOfDay = Sky.GetNode<Node>("TimeOfDay");

		Time = Cookies.User.Get<float>("Time");
		CloudPreset = Cookies.User.Get<int>("CloudPreset");
		
		SetTime(Time);
		SetCloudPreset(CloudPreset);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void SetTime(float time)
	{
		CurrentTime = time;
		Sky.Set("current_time", time);
		TimeOfDay.Set("current_time", time);
		GD.Print("Current Time Set:" + Sky.Get("current_time"));
	}
	private void SetClouds(float cirrus, float cumulus, float absorption)
	{
		SkyDome.Set("cirrus_coverage", cirrus);
		SkyDome.Set("cumulus_coverage", cumulus);

		SkyDome.Set("cirrus_absorption", absorption);
		SkyDome.Set("cumulus_absorption", absorption);
	}
	private void SetCloudPreset(int preset)
	{
		switch(preset)
		{
			case 0:
				SetClouds(0.0f, 0.0f, 2.0f);
				break;
			case 1:
				SetClouds(0.5f, 0.55f, 2.0f);
				break;
			case 2:
				SetClouds(0.7f, 0.75f, 2.0f);
				break;
			case 3:
				SetClouds(1.0f, 1.0f, 2.0f);
				break;
		}
		GD.Print($"Loaded CloudPreset: {CloudPreset}");
	}
}
