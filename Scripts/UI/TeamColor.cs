using Godot;
using System;

public partial class TeamColor : ColorPicker
{
	private TextureRect ShaderPreview;
	private ShaderMaterial RemoveBG;
	private StandardMaterial3D Team, Opponent;
	private string TeamPath = "user://Team.tres";
	private string OpponentPath = "user://Opponent.tres";
	public string SavePath = "user://Team.tres";
	public string OpponentSavePath = "user://Opponent.tres";
	private Button Save;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TreeEntered += Init;
		TreeExiting += SaveColor;
		
		Save = GetNode<Button>("%Save");
		Save.Pressed += SaveColor;
		
		Init();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Team.AlbedoColor = this.Color;
		if(RemoveBG != null) {
			RemoveBG.SetShaderParameter("replace", this.Color);
		}
	}
	public void SaveColor() {
		ResourceSaver.Save(Team, SavePath);
		ResourceSaver.Save(Opponent, OpponentSavePath);
		this.Color = Team.AlbedoColor;
	}
	public void Init() {
		ShaderPreview = GetNode<TextureRect>("%ShaderPreview");
		if(ShaderPreview.Material is ShaderMaterial shader) {
			RemoveBG = shader;
		}
		Team = ResourceLoader.Load<StandardMaterial3D>(TeamPath, "", ResourceLoader.CacheMode.Replace);
		Opponent = ResourceLoader.Load<StandardMaterial3D>(OpponentPath, "", ResourceLoader.CacheMode.Replace);
		this.Color = Team.AlbedoColor;
		/*Opponent.AlbedoColor = new Color(
			(float)GD.RandRange(0.0f, 1.0f),
			(float)GD.RandRange(0.0f, 1.0f),
			(float)GD.RandRange(0.0f, 1.0f)
		);*/
	}
}
