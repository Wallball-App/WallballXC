using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class NPCPlayer : CharacterBody3D
{
	[Export] public AnimationTree NPCAnimationTree;
	//private AnimationController AnimController;
	public Node3D NPC1, NPC2;
	//public List<Node3D> NPCS;
	public RigidBody3D Ball;
	private StaticBody3D Wall, Ground;
	private Node3D WallGeometry;
	Ball BallControl;
	private Aabb GroundAABB;
	[Export] public float WalkSpeed = 10f;
	[Export] public float BaseSpeed = 10f;
	[Export] public float Sprint = 1.6f;
	
	private Vector3 min, max;
	private Vector3 WallMin, WallMax;
	private Vector3 Margin_Min = new Vector3(0.0f, 5.0f, 0.0f);
	float margin = 15f;
	
	RandomNumberGenerator rng = new RandomNumberGenerator();
	public Vector3 Target;
	public int TEAM;
	public float Hold = -1;
	public int Frames;
	[Export] public float Max_Hold = 60;
	[Export] public float Throw_Speed = 2;
	[Export] public float Throw_Max = 4;
	[Export] public float JumpStrength = 8;
	
	public Vector3 CharacterVelocity;
	public float Dist;
	public bool PickUpRight;
	
	public float SteeringWeight = 2.0f;
	public float SteeringModifier = 20.0f;
	//public float SteeringWeight = 0.5f;
	public bool IsJumping;
	private Node3D SafePoint;
	public float WallBounce = 0.75f;
	public bool IsHolding;
	public bool IsRunning;
	public bool IsChasingBall;
	public bool WillRun;
	private float RunFrame;
	private Node3D Triangle;
	private float WorldSize;
	private StandardMaterial3D Red, Green, Yellow;
	private float Gravity;

	private int TargetFrame;

	private Node3D NpcGeometry;
	private Skeleton3D NpcSkeleton;
	private string BoneName = "mixamorig_Head";
	private int BoneIndex;

	private float ResetTime;

	public float delta;

	public GameManager.PossessionEnum Possession_Forward;
	public bool Possession0, HitWall0;
	public GameManager.ThrowerEnum Thrower_Forward;

	Vector3 pos2d, target2d;

	public enum NPCState
	{
		MOVE_TOWARD_TARGET,
		AT_TARGET,
		RUNNING,
		HOLDING,
		THROWING
	}
	public NPCState state;

	public bool CatchAnimation;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Position = new Vector3(0.0f, 0.0f, 0.0f);
		//Rotation = new Vector3(0, Mathf.DegToRad(0f), Mathf.DegToRad(-90f));


		Node Root = GetTree().Root;
		//NPCS = new List<Node3D>();
		
		/*NPC1 = Root.FindChild("Running", true, false) as Node3D;
		NPC2 = Root.FindChild("Throwing", true, false) as Node3D;
		/*NPC3 = Root.FindChild("Running", true, false) as Node3D;
		NPC4 = Root.FindChild("Running", true, false) as Node3D;
		
		//NPCS.Add(Root.FindChild("Running", true, false) as Node3D);
		//NPCS.Add(Root.FindChild("Throwing", true, false) as Node3D);
		*/
		SafePoint = Root.FindChild("SafePoint", true, false) as Node3D;
		
		Wall = Root.FindChild("Wall", true, false) as StaticBody3D;
		Ground = Root.FindChild("Ground", true, false) as StaticBody3D;
		Ball = Root.FindChild("Ball", true, false) as RigidBody3D; 
		/*NPC1 = FindChild("Running", true, false) as Node3D;
		NPC2 = FindChild("Throwing", true, false) as Node3D;
		NPC3 = FindChild("Running", true, false) as Node3D;
		NPC4 = FindChild("Running", true, false) as Node3D;
		
		SafePoint = GetNode<Node3D>("%SafePoint");
		
		Wall = GetNode<StaticBody3D>("%Wall");
		Ground = GetNode<StaticBody3D>("%Ground");*/
		
		//AnimController = FindChild("AnimationController", true, false) as AnimationController;
		//AnimController.PlayRun();
		Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

		NpcGeometry = Root.FindChild("NPC_Obj", true, false) as Node3D;
		NpcSkeleton = FindSkeleton(NpcGeometry);

		BoneIndex = NpcSkeleton.FindBone(BoneName);
		
		CollisionShape3D GroundShape = Ground.GetNode<CollisionShape3D>("GroundCollision");
		if(GroundShape.Shape is BoxShape3D s) {
			//GD.Print($"Box Size: {s.Size}");
			min = GroundShape.GlobalPosition - (s.Size / 2.0f) - new Vector3(2.0f, 0.0f, 2.0f);
			max = GroundShape.GlobalPosition + (s.Size / 2.0f) + new Vector3(2.0f, 0.0f, 2.0f);
			WorldSize = s.Size.X;
			min.Y = GroundShape.GlobalPosition.Y;
			min.Z = Wall.GlobalPosition.Z - margin;
		}
		
		CollisionShape3D WallShape = Wall.GetNode<CollisionShape3D>("WallCollision");
		if(WallShape.Shape is BoxShape3D w) {
			WallMin = WallShape.GlobalPosition - (w.Size / 2.0f) + Margin_Min;
			WallMax = WallShape.GlobalPosition + (w.Size / 2.0f);
		}
		WallGeometry = Wall.GetNode<Node3D>("WallGeometry");
		BallControl = Ball as Ball;
		
		// Remove any existing Triangle first to prevent duplicates on restart
		Node existingTriangle = FindChild("Triangle", true, false);
		if(existingTriangle != null) {
			existingTriangle.QueueFree();
		}
		
		Triangle = FindChild("Triangle", true, false).Duplicate() as Node3D;
		this.AddChild(Triangle);
		
		Red = new StandardMaterial3D();
		Red.AlbedoColor = Colors.Red;
		Red.EmissionEnabled = true;
		Red.Emission = Colors.Red;
		Red.EmissionEnergyMultiplier = 2.0f;
		
		Yellow = new StandardMaterial3D();
		Yellow.AlbedoColor = Colors.Yellow;
		Yellow.EmissionEnabled = true;
		Yellow.Emission = Colors.Yellow;
		Yellow.EmissionEnergyMultiplier = 2.0f;
		
		Green = new StandardMaterial3D();
		Green.AlbedoColor = Colors.Green;
		Green.EmissionEnabled = true;
		Green.Emission = Colors.Green;
		Green.EmissionEnergyMultiplier = 2.0f;
		
		Triangle.Visible = false;
		Frames = 0;
		Target = CreateNewTarget(false, state);
		TargetFrame = 0;
		
		rng.Randomize();

		ResetTime = rng.RandfRange(2.0f, 3.0f);

		state = NPCState.MOVE_TOWARD_TARGET;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(!Visible) return;
		if(NpcController.NPCS.Count == 0) return;
		
		this.delta = (float)delta;
		switch(state)
		{
			case NPCState.RUNNING:
				CheckWallCollide();
				MoveTowardTarget(delta);
				break;
			case NPCState.THROWING:
				ThrowBall();
				break;
			case NPCState.HOLDING:
				HoldBall();
				MoveTowardTarget(delta);
				break;
			case NPCState.AT_TARGET: 
				SetTarget();
				CheckForPossession(delta);
				break;
			case NPCState.MOVE_TOWARD_TARGET:
				MoveTowardTarget(delta);
				CheckForPossession(delta);
				CheckForTargetReset();
				break;
		}
		SetMaterial();
		ApplyGravity(delta);
		MoveAndSlide();
		UpdatePublicVariables();
		Frames++;
		GD.Print($"NPC State: {state}, Possession: {NpcController.CurrentPossession == this}, GameManager: {GameManager.possession == ((TEAM == 0) ? GameManager.PossessionEnum.TEAM : GameManager.PossessionEnum.OPPONENT)}");
	}
	
	
	
	private void RecursiveSearch(Node node, StandardMaterial3D mat) {
		if(node is MeshInstance3D m) {
			m.SetSurfaceOverrideMaterial(0, mat);
		}
		foreach(Node child in node.GetChildren()) {
			RecursiveSearch(child, mat);
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
