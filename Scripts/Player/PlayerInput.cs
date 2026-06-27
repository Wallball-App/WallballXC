using Godot;
using System;
using System.Net;

public partial class PlayerInput : CharacterBody3D
{
	[Export] public float Gravity = -9.81f;
	private float Gravity_Save;
	[Export] public float GravityScale = 2f;
	private Camera3D cam;
	private MainCamera CameraSpring;
	private float sensitivity = 0.0015f;
	private float Jump = 12f;
	private bool isJumping;
	[Export] public float WalkSpeed = 10f;
	public static bool IsSprinting;
	private CanvasLayer GameUI;
	private Vector3 DefaultPos;
	private AnimationTree PlayerAnimationTree;
	public Vector3 velocity;
	private Node3D Preview;

	public static Vector3 CamRotation;
	private Node3D PlayerGeometry;
	private VirtualJoystick Movement;
	private VirtualJoystick RotationJoystick;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cam = GetNode<Camera3D>("%MainCamera");
		CameraSpring = GetNode<MainCamera>("%CameraArm");
		PlayerGeometry = GetNode<Node3D>("%PlayerGeometry");
		GameUI = GetNode<CanvasLayer>("%GameUI");

		Movement = GetNode<CanvasLayer>("%GameUI").FindChild("Movement", true, false) as VirtualJoystick;
		RotationJoystick = GetNode<CanvasLayer>("%GameUI").FindChild("Rotation", true, false) as VirtualJoystick;

		Movement.Visible = OS.HasFeature("mobile");
		RotationJoystick.Visible = OS.HasFeature("mobile");
      
		Input.MouseMode = Input.MouseModeEnum.Captured;

		DefaultPos = cam.Position;
		Gravity_Save = Gravity;
		Preview = GetNode<Node3D>("%Preview");
		PlayerAnimationTree = GetNode<AnimationTree>("PlayerGeometry/PlayerAnimTree");
		PlayerAnimationTree.Active = true;
		CutScenePosition();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector3 movement = Vector3.Zero;;
		IsSprinting = Input.IsActionPressed("Sprint");
		WalkSpeed = (IsSprinting) ? 25f : 15f;
		
		velocity = Velocity;
		velocity = new Vector3(0.0f, velocity.Y, 0.0f);

		Vector2 keys = Input.GetVector("Left", "Right", "Forward", "Backward");

		movement = cam.GlobalTransform.Basis * new Vector3(keys.X, 0, keys.Y);
//FIX THIS PART
		Vector2 movepos = Movement.GetCombinedPivotOffset();
		movepos *= 0.1f;
		if(movepos != Vector2.Zero) movement = cam.GlobalTransform.Basis * new Vector3(movepos.X, 0, movepos.Y);

		Vector2 rot = RotationJoystick.Get("value").As<Vector2>();
		rot *= 5.0f;
		
		cam.RotateX(rot.Y * sensitivity);
		RotateY(rot.X * sensitivity);
		PlayerGeometry.RotateY(rot.X * sensitivity);
		Vector3 camrot = cam.Rotation;
		camrot.Z = 0;
		camrot.X = (float) Math.Clamp(camrot.X, -Math.PI/2, Math.PI/2);
		cam.Rotation = camrot;


		movement.Y = 0;
		velocity += movement * WalkSpeed;
		if(isJumping && IsOnFloor()) {
			velocity.Y = Jump;
			isJumping = false;
		}
		if(!IsOnFloor()) {
			velocity.Y += Gravity * (float)delta * GravityScale;
		}
		else {
			if(velocity.Y < 0) {
				velocity.Y = 0;
			}
		}
		//if(velocity.Length() > 0 && IsOnFloor()) CameraBounce(0.05f * (WalkSpeed/15f), 0.5f);
		//else if(GameManager.perspective == GameManager.CameraPerspectiveEnum.FIRST_PERSON) cam.Position = cam.Position.Lerp(DefaultPos, (float)delta * 10.0f);

		PlayerAnimationTree.Set("parameters/conditions/ThrowClicked", MousePick.IsThrowClicked);
		PlayerAnimationTree.Set("parameters/conditions/Running", velocity.Length() >= 0.25f);
		PlayerAnimationTree.Set("parameters/conditions/Stopped", velocity.Length() < 0.25f);
		if(MousePick.IsThrowClicked) MousePick.IsThrowClicked = false;
		CamRotation = cam.GlobalRotation;
		
		Velocity = velocity;
		MoveAndSlide();
	}
	public override void _Input(InputEvent @e) {
		if(@e is InputEventMouseMotion mm) {
			cam.RotateX(mm.Relative.Y * sensitivity);
			
			RotateY(-mm.Relative.X * sensitivity);
			//PlayerGeometry.RotateY(-mm.Relative.X * sensitivity);
			Vector3 rot = cam.Rotation;
			rot.Z = 0;
			rot.X = (float) Math.Clamp(rot.X, -Math.PI/2, Math.PI/2);
			cam.Rotation = rot;
		}
		if(@e.IsActionPressed("ui_cancel")) {
			if(!CameraSpring.IsInPreview) {
				Input.MouseMode = (Input.MouseMode == Input.MouseModeEnum.Captured) ? 
					Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
			} else {
				CameraSpring.Reparent();
			}
		}
		if(@e.IsActionPressed("Jump") && !@e.IsEcho() && IsOnFloor()) {
			isJumping = true;
		}
		if(@e.IsActionPressed("UI_Toggle")) {
			GameUI.Visible = (GameUI.Visible) ? false : true;
		}
		if(@e.IsActionPressed("Perspective_Change", false))
		{
			switch (GameManager.perspective)
			{
				case GameManager.CameraPerspectiveEnum.FIRST_PERSON:
					GameManager.perspective = GameManager.CameraPerspectiveEnum.THIRD_PERSON;
					RotateY((float)Math.PI);
					cam.RotateY((float)Math.PI);
					break;
				case GameManager.CameraPerspectiveEnum.THIRD_PERSON:
					GameManager.perspective = GameManager.CameraPerspectiveEnum.FIRST_PERSON;
					RotateY((float)Math.PI);
					cam.RotateY((float)Math.PI);
					break;
			}
			GetViewport().SetInputAsHandled();
		}
	}
	private void CameraBounce(float Amp, float Freq) {
		Vector3 offset = new Vector3((float)Math.Abs(0.5 * Math.Cos(GameManager.FRAMES * Freq)), (float) Math.Sin(GameManager.FRAMES * Freq), 0.0f) * Amp;
		cam.Position = cam.Position + offset;
	}
	private void CutScenePosition() {
		//Gravity = 0;
		//GlobalPosition = new Vector3(0.0f, 30.0f, -150.0f);
	}
	private void Start() {
		//Gravity = Gravity_Save;
		//GlobalPosition = new Vector3(0.0f, 1.0f, 0.0f);
	}
}
