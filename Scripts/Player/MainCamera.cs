using Godot;
using GodotCookies;
using System;
using System.Runtime.CompilerServices;

public partial class MainCamera : SpringArm3D
{
	private Node3D PlayerGeometry;
	private Skeleton3D PlayerSkeleton;
	private string BoneName = "mixamorig_Head";
	private int BoneIndex;
	private Camera3D Camera;
	private Node3D Preview;
	public bool IsInPreview = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlayerGeometry = GetNode<Node3D>("%PlayerGeometry");
		PlayerSkeleton = FindSkeleton(PlayerGeometry);
		Camera = GetNode<Camera3D>("%MainCamera");
		Preview = GetNode<Node3D>("%Preview");
		(Preview as Preview).CutsceneEnded += Reparent;

		BoneIndex = PlayerSkeleton.FindBone(BoneName);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(IsInPreview)
		{
			GlobalPosition = Preview.GlobalPosition;
			GlobalRotation = Preview.GlobalRotation;
			Camera.GlobalRotation = new Vector3(Preview.GlobalRotation.X, Camera.GlobalRotation.Y, Camera.GlobalRotation.Z);
			return;
		}
		if(GameManager.perspective == GameManager.CameraPerspectiveEnum.FIRST_PERSON)
		{
			Vector3 BonePos = PlayerSkeleton.ToGlobal(PlayerSkeleton.GetBoneGlobalPose(BoneIndex).Origin);
			GlobalPosition = BonePos;
			GlobalPosition -= GlobalTransform.Basis.Z;
			SpringLength = 1.0f;
			GlobalPosition += GlobalTransform.Basis.Z.Normalized() * (SpringLength/2.0f); //CHANGE TO CAMERA3D IF NECESSARY
			if(Camera.GlobalRotationDegrees.X > 0.0f) PlayerGeometry.Visible = false;
			else PlayerGeometry.Visible = true;
		} else if(GameManager.perspective == GameManager.CameraPerspectiveEnum.THIRD_PERSON)
		{
			Vector3 BonePos = PlayerSkeleton.ToGlobal(PlayerSkeleton.GetBoneGlobalPose(BoneIndex).Origin);
			SpringLength = 10.0f;
			GlobalPosition = BonePos;
			GlobalPosition -= GlobalTransform.Basis.Z;
			GlobalPosition -= GlobalTransform.Basis.Z.Normalized() * (SpringLength/2.0f);
		}
		
		//GD.Print(Camera.GlobalRotationDegrees.X);
	}
	private Skeleton3D FindSkeleton(Node root)
	{
		if(root is Skeleton3D) return root as Skeleton3D;
		else
		{
			foreach(Node child in root.GetChildren())
			{
				Node result = FindSkeleton(child);
				if(result is Skeleton3D skeleton) return skeleton;
			}
		}
		return null;
	}
	private void RayCastCollision()
	{
		var SpaceState = GetWorld3D().DirectSpaceState;

		Vector3 from = GlobalPosition;
		Vector3 to = GlobalPosition + GlobalRotation * 100f;

		var query = PhysicsRayQueryParameters3D.Create(from, to);
		var result = SpaceState.IntersectRay(query);

		if(result.Count > 0)
		{
			if(result["collider"].As<Node>() is CollisionShape3D)
			{
				PlayerGeometry.Visible = false;
			} else PlayerGeometry.Visible = true;
		}
 	}
	public void Reparent()
	{
		IsInPreview = false;
		GlobalRotation = Vector3.Zero;
	}
}

