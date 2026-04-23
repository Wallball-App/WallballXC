using Godot;
using System;
using System.Collections.Generic;

public partial class NpcController : Node3D
{
	[Export] public Node3D Clone;
	//[Export] public int Player_Count;
	public static int TeamCount, OpponentCount;
	[Export] public Node3D TeamController, OpponentController;
	public static List<NPCPlayer> NPCS = new List<NPCPlayer>();
	public static NPCPlayer CurrentPossession = null;
	private string TeamMaterialLoadPath, OpponentMaterialLoadPath;
	private string CountLoadPath;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TreeExiting += Clear;
		Init();
		GD.Print("Functions Bound to NPCController");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void RecursiveSearch(Node node, string Path) {
		if(node is MeshInstance3D m) {
			int surfaces = m.GetSurfaceOverrideMaterialCount();
			for(int i=0; i<surfaces; i++) {
				Material surfacemat = m.GetActiveMaterial(i);
				if(surfacemat.ResourceName == "Clothing") {
					m.SetSurfaceOverrideMaterial(i, ResourceLoader.Load<Material>(Path, "", ResourceLoader.CacheMode.Replace));
				}
			}
		}
		foreach(Node child in node.GetChildren()) {
			RecursiveSearch(child, Path);
		}
	}
	public void Init() {
		GD.Print("Initialization Started");
		TeamMaterialLoadPath = (FileAccess.FileExists("user://Team.tres")) ? 
					"user://Team.tres" : "res://Materials/Team.tres";
		OpponentMaterialLoadPath = (FileAccess.FileExists("user://Opponent.tres")) ? 
					"user://Opponent.tres" : "res://Materials/Opponent.tres";
		CountLoadPath = (FileAccess.FileExists("user://transition.cfg")) ? 
		"user://transition.cfg" : null;
		if(CountLoadPath != null) {
			ConfigFile cfg = new ConfigFile();
			cfg.Load(CountLoadPath);
			TeamCount = (int)cfg.GetValue("Players", "Team", 3);
			OpponentCount = (int)cfg.GetValue("Players", "Opponents", 3);
			GD.Print("ConfigFile Found");
		} else GD.Print("ConfigFile Not Found");
		GD.Print("Using Team Material: " + TeamMaterialLoadPath);
		GD.Print("Using Opponent Material: " + OpponentMaterialLoadPath);
		for(int i = 0; i < (TeamCount-1); i++) {   //PLAYER COUNTS AS TEAM
			Node3D ClonedNode = Clone.Duplicate() as Node3D;
			ClonedNode.Visible = true;
			RecursiveSearch(ClonedNode, TeamMaterialLoadPath);
			TeamController.AddChild(ClonedNode);
			(ClonedNode as NPCPlayer).TEAM = 0;
			NPCS.Add(ClonedNode as NPCPlayer);
			GD.Print("Adding Team");
		}
		for(int i = 0; i < (OpponentCount-1); i++) { 	//MAIN NPC IS PART OF OPPONENTS
			Node3D ClonedNode = Clone.Duplicate() as Node3D;
			ClonedNode.Visible = true;
			RecursiveSearch(ClonedNode, OpponentMaterialLoadPath);
			OpponentController.AddChild(ClonedNode);
			(ClonedNode as NPCPlayer).TEAM = 1;
			NPCS.Add(ClonedNode as NPCPlayer);
			GD.Print("Adding Opponents");
		}
	}
	public void Clear() {
		foreach(Node child in TeamController.GetChildren()) {
			child.QueueFree();
		}
		foreach(Node child in OpponentController.GetChildren()) {
			child.QueueFree();
		}
		NPCS.Clear();
	}
}
