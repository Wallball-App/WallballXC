using Godot;
using System;

public partial class PlayerModel : Node3D
{
	private Camera3D Camera;
	private string MaterialLoadPath;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Camera = GetNode<Camera3D>("%MainCamera");

		MaterialLoadPath = (FileAccess.FileExists("user://Team.tres")) ? 
					"user://Team.tres" : "res://Materials/Team.tres";
		RecursiveSearch(this, MaterialLoadPath);
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
}
