using Godot;
using System;

public partial class MapSelector : MenuButton
{
	private TextureRect Selected;
	private Button Left;
	private Button Right;
	private Button PlayButton;
	private Label Name;
	
	private int Index;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Selected = GetNode<TextureRect>("%Selected");
		Left = GetNode<Button>("%Left");
		Right = GetNode<Button>("%Right");
		PlayButton = GetNode<Button>("%PlayButton");
		Name = GetNode<Label>("%Name");
		
		Left.Pressed += OnLeftClick;
		Right.Pressed += OnRightClick;
		PlayButton.Pressed += StartGame;
		
		Index = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void OnLeftClick() {
		Index -= 1;
		Texture2D texture = GD.Load(GameData.MAPS[Index].Item3) as Texture2D;
		if(texture != null) {
			Selected.Texture = texture;
		}
		Name.Text = GameData.MAPS[Index].Item1;
	}
	public void OnRightClick() {
		Index += 1;
		Texture2D texture = GD.Load(GameData.MAPS[Index].Item3) as Texture2D;
		if(texture != null) {
			Selected.Texture = texture;
		}
		Name.Text = GameData.MAPS[Index].Item1;
	}
	public void StartGame() {
		GetTree().ChangeSceneToFile(GameData.MAPS[Index].Item2);
	}
}
