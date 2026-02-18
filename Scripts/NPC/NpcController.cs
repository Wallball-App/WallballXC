using Godot;
using System;
using System.Collections.Generic;

public partial class NpcController : Node3D
{
	[Export] public Node3D CloneT, CloneO;
	[Export] public int Player_Count;
	[Export] public Node3D TeamController, OpponentController;
	public static List<NPCPlayer> NPCS = new List<NPCPlayer>();
	public static NPCPlayer CurrentPossession = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for(int i = 0; i < (Player_Count-1); i++) {
			Node3D ClonedNode = CloneT.Duplicate() as Node3D;
			ClonedNode.Visible = true;
			//RecursiveSearch(ClonedNode, "res://Materials/Team.tres");
			TeamController.AddChild(ClonedNode);
			(ClonedNode as NPCPlayer).TEAM = 0;
			NPCS.Add(ClonedNode as NPCPlayer);
		}
		for(int i = 0; i < (Player_Count); i++) {
			Node3D ClonedNode = CloneO.Duplicate() as Node3D;
			ClonedNode.Visible = true;
			//RecursiveSearch(ClonedNode, "res://Materials/Opponent.tres");
			OpponentController.AddChild(ClonedNode);
			(ClonedNode as NPCPlayer).TEAM = 1;
			NPCS.Add(ClonedNode as NPCPlayer);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void RecursiveSearch(Node node, string Path) {
		if(node is MeshInstance3D m) {
			Material mat = GD.Load<Material>(Path);
			m.SetSurfaceOverrideMaterial(0, mat);
		}
		foreach(Node child in node.GetChildren()) {
			RecursiveSearch(child, Path);
		}
	}
}
