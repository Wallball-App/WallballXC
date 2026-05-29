using Godot;
using System;

public partial class LightDrone : Node3D
{
	private Node Sky, SkyDome, TimeOfDay;
	private float sunaltitude;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Sky = Weather.Sky;
		SkyDome = Weather.SkyDome;
		TimeOfDay = Weather.TimeOfDay;
		sunaltitude = Math.Abs(SkyDome.Get("sun_altitude").As<float>()); //INVERSE OF BRIGHTNESS
		this.Visible = (sunaltitude > 0.75f); //IF |SUN ALTITUDE| > 0.75, THEN THE DRONE IS VISIBLE

		RecurseLightVisibility(GetTree().CurrentScene); //RECURSIVELY SET THE VISIBILITY OF ALL LIGHTS IN THE SCENE TO MATCH THE DRONE'S VISIBILITY
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	private void RecurseLightVisibility(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is Light3D light && (light.Name != "SunLight" && light.Name != "MoonLight")) //ONLY AFFECT LIGHTS THAT ARE NOT THE SUN OR MOON
			{
				light.Visible = this.Visible;
				if(this.Visible == false) light.QueueFree(); //REMOVE THE LIGHT FROM THE SCENE IF IT'S NOT VISIBLE TO IMPROVE PERFORMANCE
			}
			else
			{
				RecurseLightVisibility(child);
			}
		}
	}
}
