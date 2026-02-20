using Godot;
using System;
using System.Collections.Generic;

public partial class GameData : Node
{
	public static Dictionary<int, (string, string, string)> MAPS = new Dictionary<int, (string, string, string)> {
		{0, ("Courtyard", "res://Scenes/Maps/Courtyard.tscn", "res://Map_Icons/Courtyard_1_23_2026.png")},
		{1, ("Raft", "res://Scenes/Maps/Raft.tscn", "res://Map_Icons/Raft_2_20_2026.png")}
	};
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
