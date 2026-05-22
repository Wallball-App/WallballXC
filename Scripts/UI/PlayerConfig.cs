using Godot;
using GodotCookies;
using System;

public partial class PlayerConfig : PanelContainer
{
	private TextureRect TeamPlayer, OpponentPlayer;
	private LineEdit TeamPlayerText, OpponentPlayerText;
	private GridContainer TeamContainer, OpponentContainer;
	
	private string SettingsPath = "user://transition.cfg";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TreeExiting += Save;
		OpponentPlayer = GetNode<TextureRect>("%OpponentPlayer");
		TeamPlayer = GetNode<TextureRect>("%TeamPlayer");
		
		TeamContainer = GetNode<GridContainer>("%Teammates");
		OpponentContainer = GetNode<GridContainer>("%Opponents");
		
		TeamPlayerText = GetNode<LineEdit>("%TeamPlayerCount");
		OpponentPlayerText = GetNode<LineEdit>("%OpponentPlayerCount");
		TeamPlayerText.TextChanged += OnTeamTextChange;
		OpponentPlayerText.TextChanged += OnOpponentTextChange;
		
		OnTeamTextChange(TeamPlayerText.Text);
		OnOpponentTextChange(OpponentPlayerText.Text);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	public void OnTeamTextChange(String text) {
		foreach(Node child in TeamContainer.GetChildren()) {
			child.QueueFree();
		}
		int count = int.Parse(text);
		for(int i = 0; i < count; i++) {
			TextureRect clone = TeamPlayer.Duplicate() as TextureRect;
			clone.Visible = true;
			TeamContainer.AddChild(clone);
		}
		Save();
	}
	public void OnOpponentTextChange(String text) {
		foreach(Node child in OpponentContainer.GetChildren()) {
			child.QueueFree();
		}
		int count = int.Parse(text);
		for(int i = 0; i < count; i++) {
			TextureRect clone = OpponentPlayer.Duplicate() as TextureRect;
			clone.Visible = true;
			OpponentContainer.AddChild(clone);
		}
		Save();
	}
	private void Save() {
		Cookies.User.Set("Teamcount", int.Parse(TeamPlayerText.Text));
		Cookies.User.Set("Opponentcount", int.Parse(OpponentPlayerText.Text));
	}
}
