using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NPCPlayer : Node3D
{
	private AnimationPlayer animation;
	private AnimationController AnimController;
	[Export] public Node3D NPC1, NPC2;
	[Export] public RigidBody3D Ball;
	[Export] public StaticBody3D Wall, Ground;
	private Node3D WallGeometry;
	Ball BallControl;
	private Aabb GroundAABB;
	[Export] public float WalkSpeed = 8f;
	[Export] public float BaseSpeed = 8f;
	[Export] public float Sprint = 2.0f;
	
	private Vector3 min, max;
	private Vector3 WallMin, WallMax;
	private Vector3 Margin_Min = new Vector3(0.0f, 5.0f, 0.0f);
	float margin = 15f;
	
	RandomNumberGenerator rng = new RandomNumberGenerator();
	private Vector3 Target;
	public int TEAM;
	private float Hold = -1;
	private int Frames;
	[Export] public float Max_Hold = 60;
	[Export] public float Throw_Speed = 2;
	[Export] public float Throw_Max = 4;
	[Export] public Node3D SafePoint;
	public float WallBounce = 0.75f;
	public bool IsHolding;
	public bool IsRunning;
	private Node3D Triangle;
	private StandardMaterial3D Red, Green, Yellow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Position = new Vector3(0.0f, 0.0f, 0.0f);
		//Rotation = new Vector3(0, Mathf.DegToRad(0f), Mathf.DegToRad(-90f));
		AnimController = FindChild("AnimationController", true, false) as AnimationController;
		AnimController.PlayRun();
		
		CollisionShape3D GroundShape = Ground.GetNode<CollisionShape3D>("GroundCollision");
		if(GroundShape.Shape is BoxShape3D s) {
			//GD.Print($"Box Size: {s.Size}");
			min = GroundShape.GlobalPosition - (s.Size / 2.0f) + new Vector3(2.0f, 0.0f, 2.0f);
			max = GroundShape.GlobalPosition + (s.Size / 2.0f) - new Vector3(2.0f, 0.0f, 2.0f);
			
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
		Triangle = FindChild("Triangle", true, false) as Node3D;
		
		Red = new StandardMaterial3D();
		Red.AlbedoColor = Colors.Red;
		
		Yellow = new StandardMaterial3D();
		Yellow.AlbedoColor = Colors.Yellow;
		
		Green = new StandardMaterial3D();
		Green.AlbedoColor = Colors.Green;
		
		Triangle.Visible = false;
		//Triangle.GlobalPosition = GlobalPosition + new Vector3(0.0f, 5.0f, 0.0f);
		Frames = 0;
		Target = CreateNewTarget(false, IsRunning);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!Visible) return;
		MoveTowardTarget(delta);
		CheckForThrow();
		HoldBall();
		if(IsRunning) CheckWallCollide();
		SetMaterial();
		Frames++;
	}
	private void MoveTowardTarget(double delta) {
		if(GlobalPosition.DistanceTo(Target) <= 1.0f) {
			SetPossession();
			if(GameManager.possession == GameManager.PossessionEnum.NONE && !GameManager.HitWall) {
				if(!IsRunning) Target = PredictTrajectory();
				WalkSpeed = BaseSpeed * Sprint;
			}
			else {
				Target = CreateNewTarget(GameManager.HitWall, IsRunning);
			}
		} else {
			if(GameManager.HitWall) SetPossession();
			GlobalPosition = GlobalPosition.MoveToward(Target, WalkSpeed * (float) delta);
			Vector3 d = Target - GlobalPosition;
			//if(!IsHolding) Rotation = new Vector3(0, Mathf.Atan2(d.Z, d.X), 0);
		}
	}
	private Vector3 CreateNewTarget(bool Wall, bool IsRunning) {
		if(IsRunning) return SafePoint.GlobalPosition + new Vector3(0f, 0f, 2f);
		else if(Wall || NpcController.NPCS.Where(n => n.IsRunning).Count() > 0) {
			Vector3 pos = Ball.GlobalPosition;
			pos.Y = GlobalPosition.Y;
			WalkSpeed = BaseSpeed * Sprint;
			return pos;
		}
		else {
			rng.Randomize();
			WalkSpeed = BaseSpeed;
			return new Vector3(rng.RandfRange(min.X, max.X), GlobalPosition.Y,
				rng.RandfRange(min.Z, max.Z));
		} 
	}
	private void SetPossession() {
		if(Ball.GlobalPosition.DistanceTo(GlobalPosition) <= 5f && !IsHolding) {
			rng.Randomize();
			if(!IsRunning && 
					rng.Randf() <= 0.7f && GameManager.possession == GameManager.PossessionEnum.NONE) {
				//GD.Print(rng.Randf());
				BallControl.Catch((TEAM == 0) ? GameManager.PossessionEnum.TEAM : GameManager.PossessionEnum.OPPONENT, 
					GlobalPosition + new Vector3(0.0f, 0.0f, 1.0f));
					rng.Randomize();
				if(TEAM == 0 && NpcController.NPCS.Where(n => n.TEAM == 1 && n.IsRunning).Count() > 0) Hold = Frames + 1;
				else if(TEAM == 1 && NpcController.NPCS.Where(n => n.TEAM == 0 && n.IsRunning).Count() > 0) Hold = Frames + 1;
				else Hold = Frames + (rng.Randf() * Max_Hold);
				if(NpcController.CurrentPossession == null) {
					NpcController.CurrentPossession = this;
					IsHolding = true;
				}
			}
			else {
				IsRunning = true;
				BallControl.Throw((TEAM == 0) ? GameManager.ThrowerEnum.TEAM : GameManager.ThrowerEnum.OPPONENT, 
							Vector3.Down, 1);
				Target = CreateNewTarget(GameManager.HitWall, IsRunning);
			}
		}
	}
	private void CheckForThrow() {
		if(Frames >= Hold && Hold != -1) {
			Hold = -1;
			IsHolding = false;
			rng.Randomize();
			AnimController.PlayThrow();
			float DirectionX = rng.RandfRange(WallMin.X, WallMax.X);
			float DirectionY = rng.RandfRange(WallMin.Y + 1.0f, WallMax.Y - 5.0f);
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
		}
	}
	private void CheckWallCollide() {
		if(GlobalPosition.Z > SafePoint.GlobalPosition.Z) {
			if(IsRunning) IsRunning = false;
			Target = CreateNewTarget(false, IsRunning);
			//GD.Print("NPC Safe");
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
					if(GameManager.possession == GameManager.PossessionEnum.TEAM
						|| GameManager.possession == GameManager.PossessionEnum.PLAYER) RecursiveSearch(Triangle, Yellow);
					else RecursiveSearch(Triangle, Red);
				}
			} else {
				if(GameManager.possession == GameManager.PossessionEnum.OPPONENT) {
					RecursiveSearch(Triangle, Yellow);
				} else {
					if(GameManager.thrower == GameManager.ThrowerEnum.OPPONENT) RecursiveSearch(Triangle, Yellow);
					else RecursiveSearch(Triangle, Red);
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
		Vector3 f = Ball.GlobalPosition;
		Vector3 t = Ball.GlobalPosition + (lvel * 1000f);
		
		var spacestate = PhysicsServer3D.Singleton.SpaceGetDirectState(GetWorld3D().Space);
		var q = PhysicsRayQueryParameters3D.Create(f, t);
		var result = spacestate.IntersectRay(q);
		Vector3 CollisionPoint;
		if(result.Count > 0) {
			CollisionPoint = (Vector3)result["position"];
			float dist = f.DistanceTo(CollisionPoint);
			Vector3 negative_velocity = lvel.Bounce(CollisionPoint.Normalized());
			Vector3 final = CollisionPoint + negative_velocity;
			final.Y = GlobalPosition.Y;
			return final;
		} else {
			return Ball.GlobalPosition + new Vector3(0.0f, -Ball.GlobalPosition.Y, 5.0f);
		}
	}
	
}
