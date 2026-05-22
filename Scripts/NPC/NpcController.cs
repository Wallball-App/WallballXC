using Godot;
using GodotCookies;
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
	
	private Timer CutScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TreeExiting += Clear;
		NPCS.Clear();
		CurrentPossession = null;
		
		CutScene = GetNode<Timer>("%CutsceneTimer");
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
	private void ResetClone(Node3D node)
	{
		foreach(Node child in node.GetChildren()) {
			if(child is Node3D child3D) {
				ResetClone(child3D);
			}
		}
		if(node is CharacterBody3D body) {
			body.Velocity = Vector3.Zero;
		}
		if(node is NPCPlayer npc) {
			npc.IsHolding = false;
			npc.IsRunning = false;
			npc.IsJumping = false;
			npc.Hold = -1;
			npc.Frames = 0;
			npc.Target = npc.GlobalPosition;
		}
		// Reset Triangle visibility and clear emissive materials
		if(node.Name == "Triangle") {
			node.Visible = false;
			node.GlobalPosition = Vector3.Zero;
			// Clear any emissive materials applied to Triangle meshes
			if(node is MeshInstance3D mesh) {
				for(int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++) {
					mesh.SetSurfaceOverrideMaterial(i, null);
				}
			}
		}
	}
	public void Init() {
		GD.Print("--------------------");
		ResetClone(Clone);
		Clone.Visible = false;
		Clone.SetPhysicsProcess(false);
		GD.Print("Initialization Started");
		TeamMaterialLoadPath = (FileAccess.FileExists("user://Team.tres")) ? 
					"user://Team.tres" : "res://Materials/Team.tres";
		OpponentMaterialLoadPath = (FileAccess.FileExists("user://Opponent.tres")) ? 
					"user://Opponent.tres" : "res://Materials/Opponent.tres";

		TeamCount = Cookies.User.Get<int>("Teamcount");
		if(TeamCount <= 0) TeamCount = 3;
		OpponentCount = Cookies.User.Get<int>("Opponentcount");
		if(OpponentCount <= 0) OpponentCount = 3;
		
		GD.Print("Using Team Material: " + TeamMaterialLoadPath);
		GD.Print("Using Opponent Material: " + OpponentMaterialLoadPath);
		for(int i = 0; i < (TeamCount-1); i++) {   //PLAYER COUNTS AS TEAM
			Node3D ClonedNode = Clone.Duplicate() as Node3D;
			ClonedNode.Visible = true;
			Clone.SetPhysicsProcess(true);
			RecursiveSearch(ClonedNode, TeamMaterialLoadPath);
			TeamController.AddChild(ClonedNode);
			(ClonedNode as NPCPlayer).TEAM = 0;
			NPCS.Add(ClonedNode as NPCPlayer);
			GD.Print("Adding Team");
		}
		for(int i = 0; i < (OpponentCount-1); i++) { 	//MAIN NPC IS PART OF OPPONENTS
			Node3D ClonedNode = Clone.Duplicate() as Node3D;
			ClonedNode.Visible = true;
			Clone.SetPhysicsProcess(true);
			RecursiveSearch(ClonedNode, OpponentMaterialLoadPath);
			OpponentController.AddChild(ClonedNode);
			(ClonedNode as NPCPlayer).TEAM = 1;
			NPCS.Add(ClonedNode as NPCPlayer);
			GD.Print("Adding Opponents");
		}
		Clone.Visible = true;
		Clone.SetPhysicsProcess(true);
	}
	public void Clear() {
		foreach(Node child in TeamController.GetChildren()) {
			child.Free();
		}
		foreach(Node child in OpponentController.GetChildren()) {
			child.Free();
		}
		NPCS.Clear();
	}
}
