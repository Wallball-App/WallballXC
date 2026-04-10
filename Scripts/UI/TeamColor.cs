using Godot;
using System;

public partial class TeamColor : ColorPicker
{
	private TextureRect ShaderPreview;
	private ShaderMaterial RemoveBG;
	private StandardMaterial3D Team;
	private string TeamPath = "res://Materials/Team.tres";
	private Button Save;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ShaderPreview = GetNode<TextureRect>("%ShaderPreview");
		if(ShaderPreview.Material is ShaderMaterial shader) {
			RemoveBG = shader;
		}
		Team = GD.Load<StandardMaterial3D>(TeamPath);
		Save = GetNode<Button>("%Save");
		Save.Pressed += SaveColor;
		this.Color = Team.AlbedoColor;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Team.AlbedoColor = this.Color;
		if(RemoveBG != null) {
			RemoveBG.SetShaderParameter("replace", this.Color);
		}
	}
	private void SaveColor() {
		ResourceSaver.Save(Team, TeamPath);
	}
}
