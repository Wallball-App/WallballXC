using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;

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
	[Export] public float WalkSpeed = 6f;
	[Export] public float BaseSpeed = 12f;
	[Export] public float Sprint = 2.0f;
	
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
		Target = CreateNewTarget(false, IsRunning);
		TargetFrame = 0;
		
		rng.Randomize();

		ResetTime = rng.RandfRange(2.0f, 3.0f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(!Visible) return;
		if(NpcController.NPCS.Count == 0) return;
		this.delta = (float)delta;
		MoveTowardTarget(delta);
		CheckForThrow();
		HoldBall();
		if(IsRunning) CheckWallCollide();
		//CheckForRunning();
		SetMaterial();
		ApplyGravity(delta);
		MoveAndSlide();
		UpdatePublicVariables();
		Frames++;
	}
	private void MoveTowardTarget(double delta) {
		if(/*GlobalPosition.DistanceTo(Target) <= 3.0f || */Frames >= TargetFrame + (int)(ResetTime/delta)) {

			ResetTime = rng.RandfRange(1.0f, 3.0f); //SET RANDOM RESET

			if(GameManager.possession == GameManager.PossessionEnum.NONE) SetPossession((float)delta); //CHECK FOR POSSESSION
			
			if(GameManager.possession == GameManager.PossessionEnum.NONE && !GameManager.HitWall) { //PREDICT TRAJECTORY OF BALL
				if(!IsRunning) Target = PredictTrajectory();
				Sprint = Dist/WorldSize + 1f;
				WalkSpeed = BaseSpeed * Sprint;
			}
			else {
				if(rng.Randf() <= 0.75f) //IDLE OR TARGET LOGIC
				{
					Target = CreateNewTarget(GameManager.HitWall, IsRunning);
				} 
				else if(Target.DistanceTo(Ball.GlobalPosition) >= 5f && 
						!GameManager.HitWall && GameManager.possession != GameManager.PossessionEnum.NONE) //Not Ball Target, Ball not Thrown, and Someone Currently Holding
				{
					Target = GlobalPosition;
					Velocity = Vector3.Zero;
				}
				
				TargetFrame = Frames; //Target-Change Frame Set
			}
			PickUpRight = (Dist <= 6.0f && 
				Target.DistanceTo(Ball.GlobalPosition) <= 0.5f && Ball.GlobalPosition.Y <= 0.75f 
				&& GameManager.HitWall && GameManager.possession == GameManager.PossessionEnum.NONE);
			

		} else {
			/*if(Target == Vector3.Zero)
			{
				Velocity -= (Velocity * 0.5f);
			}*/
			//if(Target.DistanceTo(Ball.GlobalPosition) <= 5f) GD.Print("Target Distance: " + GlobalPosition.DistanceTo(Target) + ", Target.Y: " + Ball.GlobalPosition.Y);
			if(GameManager.HitWall) SetPossession((float)delta); //Check for possession
			//GlobalPosition = GlobalPosition.MoveToward(Target, WalkSpeed * (float) delta);
			Vector3 velocity = Velocity;

			Vector3 pos2d = new Vector3(GlobalPosition.X, 0.0f, GlobalPosition.Z);
			Vector3 target2d = new Vector3(Target.X, 0.0f, Target.Z);

			if(pos2d.DistanceTo(target2d) <= 5.0f && Target.Y > GlobalPosition.Y + 4.0f)
			{
				if(IsOnFloor()) IsJumping = true;
			}
			if(IsJumping && IsOnFloor()) {
				velocity.Y = JumpStrength;
				IsJumping = false;
			} else
			{
				if(IsOnFloor()) velocity.Y = 0;
			}
			Vector3 desired = GlobalPosition.DirectionTo(new Vector3(Target.X, GlobalPosition.Y, Target.Z)) * WalkSpeed;
			//SteeringWeight = (5.0f/WalkSpeed) * (WorldSize / GlobalPosition.DistanceTo(Target));
			Vector3 steering = (desired - velocity) * (WalkSpeed/BaseSpeed) * 0.75f; //Steering = desired - current, with weight
			steering = steering.LimitLength(WalkSpeed * Math.Clamp(Dist, 0.0f, 0.75f)); //Limit steering to prevent overshooting, with distance-based scaling
			
			
			if(IsOnWall()) steering = steering - (0.5f * steering);


			Velocity += steering * (float)delta;
			Velocity = new Vector3(Velocity.X, velocity.Y, Velocity.Z); //Y is based on modified Velocity, while X and Z are steering

			if(Velocity.Length() >= 0.5f) { //Slerp Rotation to watch Target
				Basis lookat;
				if(IsHolding) {
					Vector3 wallDir = GlobalPosition - Wall.GlobalPosition;
					if(wallDir.IsZeroApprox())
					{
						wallDir = new Vector3(0.0f, 0.0f, -1.0f);
					}
					lookat = Basis.LookingAt(wallDir, Vector3.Up);
				} else {
					Vector3 v = new Vector3(-Velocity.X, 0.0f, -Velocity.Z).Normalized();
					if(v.IsZeroApprox())
					{
						v = new Vector3(0.0f, 0.0f, 1.0f);
					}
					lookat = Basis.LookingAt(v, Vector3.Up);
				}
				Basis = Basis.Orthonormalized().Slerp(lookat.Orthonormalized(), WalkSpeed * (float)delta);
			}
			//if(!IsHolding) Rotation = new Vector3(0, Mathf.Atan2(d.Z, d.X), 0);
		}
		if(GameManager.CWall && rng.Randf() <= 0.3f) { //Override NPC target to if ball hits wall and 30% chance
			if(!IsRunning) Target = CreateNewTarget(GameManager.HitWall, IsRunning);
			return;
		}
		if(GlobalPosition.DistanceTo(Target) <= 3.0f && !IsRunning && Target.DistanceTo(Ball.GlobalPosition) >= 5f) //Set Idle Animation
		{
			Target = GlobalPosition;
			Velocity = new Vector3(0.0f, Velocity.Y, 0.0f); //To Account for Gravity
			Vector3 ballPos = new Vector3(Ball.GlobalPosition.X, GlobalPosition.Y, Ball.GlobalPosition.Z);
			Basis lookat = Basis.LookingAt(ballPos, Vector3.Up);
			Basis = Basis.Orthonormalized().Slerp(lookat.Orthonormalized(), 0.5f);
		}
		Triangle.GlobalPosition = GlobalPosition + new Vector3(0.0f, 5.0f, 0.0f);

		Vector3 dir = GlobalPosition.DirectionTo(Target).Normalized();
		Vector2 relativePositionVector = new Vector2(dir.X, dir.Y);
		NPCAnimationTree.Set("parameters/MainStateMachine/NPC_Catch_Blend/blend_position", relativePositionVector);
	}
	private Vector3 CreateNewTarget(bool Wall, bool IsRunning) {
		if(IsRunning) return SafePoint.GlobalPosition + new Vector3(0f, 0f, 2f);
		else if(Wall || (NpcController.NPCS.Where(n => n.IsRunning).Count() > 0 &&
			GameManager.possession == GameManager.PossessionEnum.NONE)) {
			Vector3 pos = Ball.GlobalPosition;
			//pos.Y = GlobalPosition.Y;
			Vector3 RandomOffset = new Vector3(rng.RandfRange(-5.0f, 5.0f), 0.0f, rng.RandfRange(-5.0f, 5.0f));
			pos += RandomOffset;
			IsChasingBall = true;
			WalkSpeed = BaseSpeed * Sprint;
			return pos;
		}
		else {
			WalkSpeed = BaseSpeed;
			IsChasingBall = false;
			return new Vector3(rng.RandfRange(min.X, max.X), GlobalPosition.Y,
				rng.RandfRange(min.Z, WallMax.Z));
		} 
	}
	private void SetPossession(float delta) {
		if(Frames < 120) return;
		if(Ball.GlobalPosition.DistanceTo(GlobalPosition) <= 5f && !IsHolding) {
			if(!IsRunning && 
					rng.Randf() <= 0.7f && GameManager.possession == GameManager.PossessionEnum.NONE) {
				//GD.Print(rng.Randf());
				if(NpcController.CurrentPossession == null) {
					NpcController.CurrentPossession = this;

					IsHolding = true;

					Vector3 BonePos = NpcSkeleton.ToGlobal(NpcSkeleton.GetBoneGlobalPose(BoneIndex).Origin);
					BallControl.Catch((TEAM == 0) ? GameManager.PossessionEnum.TEAM : GameManager.PossessionEnum.OPPONENT, 
							BonePos + new Vector3(0.0f, 0.25f, 0.0f));

					if(TEAM == 0 && NpcController.NPCS.Where(n => n.TEAM == 1 && n.IsRunning).Count() > 0) Hold = Frames + (0.25f/delta);
					else if(TEAM == 1 && NpcController.NPCS.Where(n => n.TEAM == 0 && n.IsRunning).Count() > 0) Hold = Frames + (0.25f/delta);
					else Hold = Frames + (rng.Randf() * Max_Hold) / (delta * 60f) + (0.4f / delta);
				}
			}
			else {
				SetRunning();
			}
		}
	}
	private void CheckForThrow() {
		if(Frames >= Hold && Hold != -1) {
			Hold = -1;
			IsHolding = false;
			//AnimController.PlayThrow();
			float DirectionX = rng.RandfRange(WallMin.X, WallMax.X);
			float DirectionY = rng.RandfRange(GlobalPosition.Y + 5.0f, WallMax.Y - 5.0f);
			float DirectionZ = WallMax.Z;
			Vector3 Point = new Vector3(DirectionX, DirectionY, DirectionZ);
			Vector3 Direction = (Point - GlobalPosition).Normalized();
			float Speed = rng.RandfRange(Throw_Speed, Throw_Max);
			BallControl.Throw((TEAM == 0) ? GameManager.ThrowerEnum.TEAM : GameManager.ThrowerEnum.OPPONENT, 
				Direction, Speed);
			if(NpcController.CurrentPossession == this) NpcController.CurrentPossession = null;
		}
	}
	private void HoldBall() {
		//IsHolding = (IsHolding && Frames < Hold);
		if(IsHolding) {
			Ball.GlobalPosition = GlobalPosition + new Vector3(0.0f, 2.0f, 1.0f);
			if(GlobalPosition.Z > SafePoint.GlobalPosition.Z) {
				Target = CreateNewTarget(GameManager.HitWall, IsRunning);
				TargetFrame = Frames;
			}
		}
	}
	private void CheckWallCollide() {
		if(GlobalPosition.Z > SafePoint.GlobalPosition.Z) {
			if(IsRunning) IsRunning = false;
			Target = CreateNewTarget(GameManager.HitWall, IsRunning); //Change HitWall to FALSE to change NPC Behavior
			//GD.Print("NPC Safe");
			TargetFrame = Frames;
		} 
	}
	private void RecursiveSearch(Node node, StandardMaterial3D mat) {
		if(node is MeshInstance3D m) {
			m.SetSurfaceOverrideMaterial(0, mat);
		}
		foreach(Node child in node.GetChildren()) {
			RecursiveSearch(child, mat);
		}
	}
	private void SetMaterial() {
		if(IsRunning) {
			Triangle.Visible = true;
			if(TEAM == 0) {
				if(GameManager.possession == GameManager.PossessionEnum.TEAM
						|| GameManager.possession == GameManager.PossessionEnum.PLAYER) {
					RecursiveSearch(Triangle, Yellow);
				} else {
					RecursiveSearch(Triangle, Red);
				}
			} else {
				if(GameManager.possession == GameManager.PossessionEnum.OPPONENT) {
					RecursiveSearch(Triangle, Yellow);
				} else {
					RecursiveSearch(Triangle, Red);
				}
			}
		} else if(NpcController.CurrentPossession == this){
			Triangle.Visible = true;
			RecursiveSearch(Triangle, Green);
		} else {
			Triangle.Visible = false;
		}
		
	}
	private Vector3 PredictTrajectory() {
		Vector3 lvel = Ball.LinearVelocity;
		Vector3 pos = Ball.GlobalPosition;

		//float angle = Mathf.Atan2(lvel.Z, lvel.X);
		float dist = pos.DistanceTo(Wall.GlobalPosition);

		float vz = lvel.Z; //Perpendicular velocity at collision
		if(vz <= 0) return Ball.GlobalPosition + new Vector3(0.0f, -Ball.GlobalPosition.Y, 5.0f); //If ball is moving away from wall, return current position as target
		
		float vx = lvel.X; //X axis velocity
		float time = dist/vz;

		float vy = lvel.Y - (Gravity * time); //Vertical velocity at collision, accounting for gravity

		Vector3 bouncedvector = new Vector3(vx, vy * WallBounce, -vz * WallBounce); //VELOCITY VECTOR AFTER BOUNCE

		float collisionX = pos.X + (lvel.X * time); //X position at collision
		float collisionY = pos.Y + (lvel.Y * time) - (0.5f * Gravity * (time * time)); //Y position at collision, accounting for gravity, kinematic equation
		float collisionZ = WallMin.Z; //Z position at collision (wall position)

		float timetoground = TimeKinematic(0.5f, collisionY, bouncedvector.Y); //Time for ball to hit ground after collision, using kinematic equation
		float groundZ = PositionKinematic(timetoground, collisionZ, bouncedvector.Z); //Z position when ball hits ground after collision, using kinematic equation
		float groundX = PositionKinematic(timetoground, collisionX, bouncedvector.X); //X position when ball hits ground after collision, using kinematic equation

		Vector3 result = new Vector3(groundX, GlobalPosition.Y, groundZ);
		if(!result.IsFinite()) return Ball.GlobalPosition + new Vector3(0.0f, -Ball.GlobalPosition.Y, 5.0f);
				/*Vector3 t = Ball.GlobalPosition + (lvel * 1000f);
		var spacestate = PhysicsServer3D.Singleton.SpaceGetDirectState(GetWorld3D().Space);
		var q = PhysicsRayQueryParameters3D.Create(pos, t);
		var result = spacestate.IntersectRay(q);
		Vector3 CollisionPoint;
		if(result.Count > 0) {
			CollisionPoint = (Vector3)result["position"];
			Vector3 normal = (Vector3)result["normal"];
			float collisiondist = pos.DistanceTo(CollisionPoint);
			Vector3 negative_velocity = (lvel.Normalized() * wallspeed).Bounce(normal);
			Vector3 final = CollisionPoint + negative_velocity * 0.5f;
			final.Y = GlobalPosition.Y;
			return final;
		} else {
			return Ball.GlobalPosition + new Vector3(0.0f, -Ball.GlobalPosition.Y, 5.0f);
		}*/
		return result;
	}
	
	private void ApplyGravity(double delta) {
		if(!IsOnFloor() || IsJumping) {
			Vector3 velocity = Velocity;
			velocity.Y -= Gravity * (float)delta;
			Velocity = velocity;
		}
	}
	private Vector3 FindNearestForward() {
		Vector3 MinDist = Vector3.Zero;
		foreach(NPCPlayer N in NpcController.NPCS.Where(n => n.GlobalPosition.Z > GlobalPosition.Z)) {
			if(MinDist == Vector3.Zero) MinDist = N.GlobalPosition - GlobalPosition;
			if((N.GlobalPosition - GlobalPosition).Length() < MinDist.Length()) MinDist = N.GlobalPosition - GlobalPosition;
		}
		return MinDist;
	}
	/*private void SwapLists(int index) {
		foreach(Node3D npc in NPCS) {
			if(npc == NPCS[index]) npc.Visible = true;
			else if(npc != NPCS[index]) npc.Visible = false;
		}
	}*/
	public void UpdatePublicVariables() {
		CharacterVelocity = Velocity;
		Dist = GlobalPosition.DistanceTo(Target);
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
	private void CheckForRunning()
	{
		if(Frames >= RunFrame && RunFrame != 0) SetRunning();
	}
	private void SetRunning()
	{
		IsRunning = true;
		BallControl.Throw((TEAM == 0) ? GameManager.ThrowerEnum.TEAM : GameManager.ThrowerEnum.OPPONENT, 
					Vector3.Down, 1);
		Target = CreateNewTarget(GameManager.HitWall, IsRunning);
		TargetFrame = Frames;
	}
	private float TimeKinematic(float desired, float x0, float v0) //Returns the time it would take for the ball to reach the desired position
	{
		float t = v0 - Mathf.Sqrt((v0*v0) + (2*Gravity*(x0 - desired)));
		t /= -Gravity;
		return t;
	}
	private float PositionKinematic(float time, float x0, float v0) //Returns the position of the ball at a given time, using kinematic equations
	{
		return x0 + v0 * time - 0.5f * Gravity * time * time;
	}
}
