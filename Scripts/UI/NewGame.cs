using Godot;
using System;
using System.Threading.Tasks;

public partial class NewGame : Button
{
	private OnLoad Alpha;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Pressed += OnPress;
		Alpha = GetNode<OnLoad>("%Alpha");
		Alpha.FadeIn();
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public async void OnPress() {
		await Alpha.FadeOut();
		Alpha.Color = Colors.Black;
		await Task.Delay(25);
		GetTree().ChangeSceneToFile("res://Scenes/Config/GameConfig.tscn");
	}
}
