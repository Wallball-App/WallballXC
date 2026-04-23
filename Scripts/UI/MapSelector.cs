using Godot;
using System;

public partial class MapSelector : PanelContainer
{
	private TextureRect Selected;
	private Button Left;
	private Button Right;
	private Button PlayButton;
	private Label Name;
	private OnLoad Alpha;
	
	private int Index;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Selected = GetNode<TextureRect>("%Selected");
		Left = GetNode<Button>("%Left");
		Right = GetNode<Button>("%Right");
		PlayButton = GetNode<Button>("%PlayButton");
		Name = GetNode<Label>("%Name");
		
		Alpha = GetNode<OnLoad>("%Alpha");
		
		Left.Pressed += OnLeftClick;
		Right.Pressed += OnRightClick;
		PlayButton.Pressed += StartGame;
		
		Index = 0;
		
		Alpha.FadeIn();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void OnLeftClick() {
		if(Index > 0) Index -= 1;
		Texture2D texture = GD.Load(GameData.MAPS[Index].Item3) as Texture2D;
		if(texture != null) {
			Selected.Texture = texture;
		}
		Name.Text = GameData.MAPS[Index].Item1;
	}
	public void OnRightClick() {
		if(Index <= GameData.MAPS.Count) Index += 1;
		Texture2D texture = GD.Load(GameData.MAPS[Index].Item3) as Texture2D;
		if(texture != null) {
			Selected.Texture = texture;
		}
		Name.Text = GameData.MAPS[Index].Item1;
	}
	public async void StartGame() {
		await Alpha.FadeOut();
		GetTree().CallDeferred("change_scene_to_file", GameData.MAPS[Index].Item2);
	}
}
