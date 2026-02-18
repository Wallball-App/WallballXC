using Godot;
using System;

public partial class MousePick : CollisionShape3D
{
	private RigidBody3D Ball;
	private Ball BallControl;
	private GameManager ctx;
	[Export] private float ThrowSpeed = 2f;
	private Camera3D cam;
	private Vector3 Offset = new Vector3(0.0f, 0.0f, -3.0f);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Ball = GetNode<RigidBody3D>("/root/Root/Ball");
		BallControl = GetNode<Ball>("/root/Root/Ball");
		ctx = GetNode<GameManager>("/root/Root/GameManager");
		cam = GetNode<Camera3D>("../MainCamera");
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
			Ball.GlobalPosition = cam.GlobalPosition + Offset;
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
					if(SafeText.Safe) BallControl.Catch(GameManager.PossessionEnum.PLAYER, 
						cam.GlobalPosition + Offset);
				}
			}
			if(ev.ButtonIndex == MouseButton.Left && !ev.Pressed) {
				if(GameManager.possession == GameManager.PossessionEnum.PLAYER) {
					/*GameManager.possession = GameManager.PossessionEnum.NONE;
					GameManager.thrower = GameManager.ThrowerEnum.PLAYER;
					Ball.Freeze = false;
					Ball.LinearVelocity = Vector3.Zero;
					Ball.AngularVelocity = Vector3.Zero;
					Ball.ApplyImpulse(-cam.GlobalTransform.Basis.Z * ThrowSpeed);*/
					BallControl.Throw(GameManager.ThrowerEnum.PLAYER, 
						-cam.GlobalTransform.Basis.Z, ThrowSpeed);
				}
			}
		}
	}
}
