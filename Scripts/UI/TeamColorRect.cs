using Godot;
using System;

public partial class TeamColorRect : ColorRect
{
	private StandardMaterial3D TeamColor;
	private ShaderMaterial CurrentMaterial;
	private string LoadPath;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadPath = (FileAccess.FileExists("user://Team.tres")) ? 
					"user://Team.tres" : "res://Materials/Team.tres";
		TeamColor = GD.Load<StandardMaterial3D>(LoadPath);
		CurrentMaterial = this.Material as ShaderMaterial;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CurrentMaterial.SetShaderParameter("remove_color", Colors.White);
		CurrentMaterial.SetShaderParameter("tolerance", 1f);
		CurrentMaterial.SetShaderParameter("replace", TeamColor.AlbedoColor);
	}
}
