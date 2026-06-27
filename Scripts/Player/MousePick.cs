using Godot;
using System;

public partial class MousePick : CollisionShape3D
{
	private RigidBody3D Ball;
	private Ball BallControl;
	private GameManager ctx;
	[Export] public float ThrowSpeed = 2f;
	[Export] public float Sprint_ThrowSpeed = 4f;

	public static bool IsThrowClicked;
	private Camera3D cam;
	private Node3D PlayerGeometry;
	private Vector3 Offset = new Vector3(0.0f, 0.0f, 3.0f);
	private Skeleton3D PlayerSkeleton;
	private string BoneName = "mixamorig_Head";
	private int BoneIndex;
	private Vector3 BonePos;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Ball = GetNode<RigidBody3D>("%Ball");
		BallControl = GetNode<Ball>("%Ball");
		ctx = GetNode<GameManager>("%GameManager");
		cam = GetNode<Camera3D>("%MainCamera");
		PlayerGeometry = GetNode<Node3D>("%PlayerGeometry");

		PlayerSkeleton = FindSkeleton(PlayerGeometry);
		BoneIndex = PlayerSkeleton.FindBone(BoneName);
		
		Ball.GlobalTransform = new Transform3D(Basis.Identity, new Vector3(0.0f, 10f, 0.0f));
		GameManager.possession = GameManager.PossessionEnum.PLAYER;
		GameManager.thrower = GameManager.ThrowerEnum.NONE;
		Ball.Freeze = true;
		Ball.GlobalPosition = cam.GlobalPosition + Offset;
	}
	public override void _Process(double delta) {
		if(GameManager.possession == GameManager.PossessionEnum.PLAYER) {
			/*Ball.GlobalTransform = new Transform3D(Basis.Identity, 
						cam.GlobalPosition - cam.Rotation);*/
			Vector3 pos = Vector3.Zero;
			if(GameManager.perspective == GameManager.CameraPerspectiveEnum.FIRST_PERSON)
			{
				pos = cam.GlobalPosition + Offset;
			} else if(GameManager.perspective == GameManager.CameraPerspectiveEnum.THIRD_PERSON)
			{
				pos = BonePos + new Vector3(0.0f, 0.0f, 0.5f);
			} 
			Ball.GlobalPosition = pos;
			BonePos = PlayerSkeleton.ToGlobal(PlayerSkeleton.GetBoneGlobalPose(BoneIndex).Origin);
		}
	}
	public override void _Input(InputEvent @e) {
		if(@e is InputEventMouseButton ev) {
			if(ev.ButtonIndex == MouseButton.Right && !ev.Pressed) {
				if(GlobalPosition.DistanceTo(Ball.GlobalPosition) <= 10) {
					/*GameManager.possession = GameManager.PossessionEnum.PLAYER;
					GameManager.thrower = GameManager.ThrowerEnum.NONE;
					Ball.Freeze = true;
					/*Ball.GlobalTransform = new Transform3D(Basis.Identity, 
						cam.GlobalPosition - cam.Rotation);
					Ball.GlobalPosition = cam.GlobalPosition + Offset;*/
					if(SafeText.Safe && GameManager.possession == GameManager.PossessionEnum.NONE)
					{
						Vector3 pos = Vector3.Zero;
						if(GameManager.perspective == GameManager.CameraPerspectiveEnum.FIRST_PERSON)
						{
							pos = cam.GlobalPosition + Offset;
						} else if(GameManager.perspective == GameManager.CameraPerspectiveEnum.THIRD_PERSON)
						{
							pos = BonePos + new Vector3(0.0f, 0.0f, 0.5f);
						} 
						BallControl.Catch(GameManager.PossessionEnum.PLAYER, pos);
					}
				}
			}
			if(ev.ButtonIndex == MouseButton.Left && !ev.Pressed) {
				IsThrowClicked = true;
				if(GameManager.possession == GameManager.PossessionEnum.PLAYER) {
					/*GameManager.possession = GameManager.PossessionEnum.NONE;
					GameManager.thrower = GameManager.ThrowerEnum.PLAYER;
					Ball.Freeze = false;
					Ball.LinearVelocity = Vector3.Zero;
					Ball.AngularVelocity = Vector3.Zero;
					Ball.ApplyImpulse(-cam.GlobalTransform.Basis.Z * ThrowSpeed);*/
					BallControl.Throw(GameManager.ThrowerEnum.PLAYER, 
						-cam.GlobalTransform.Basis.Z, (PlayerInput.IsSprinting ? Sprint_ThrowSpeed : ThrowSpeed));
				}
			}
		}
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
}
