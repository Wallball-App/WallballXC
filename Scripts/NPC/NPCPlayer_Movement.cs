using Godot;
using System;
using System.Linq;

public partial class NPCPlayer
{
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

			pos2d = new Vector3(GlobalPosition.X, 0.0f, GlobalPosition.Z);
			target2d = new Vector3(Target.X, 0.0f, Target.Z);

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
					wallDir.Y = 0.0f;
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

		Vector3 modGP = GlobalPosition + new Vector3(0.0f, 2.25f, 0.0f);
		Vector3 dir = modGP.DirectionTo(Target).Normalized();
		Vector2 relativePositionVector = new Vector2(dir.X, dir.Y);
		NPCAnimationTree.Set("parameters/MainStateMachine/NPC_Catch_Blend/blend_position", relativePositionVector);
		NPCAnimationTree.Set("parameters/IsJumping", IsJumping);
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
	private void CheckWallCollide() {
		if(GlobalPosition.Z > SafePoint.GlobalPosition.Z) {
			if(IsRunning) IsRunning = false;
			Target = CreateNewTarget(GameManager.HitWall, IsRunning); //Change HitWall to FALSE to change NPC Behavior
			//GD.Print("NPC Safe");
			TargetFrame = Frames;
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
	private void ApplyGravity(double delta) {
		if(!IsOnFloor() || IsJumping) {
			Vector3 velocity = Velocity;
			velocity.Y -= Gravity * (float)delta;
			Velocity = velocity;
		}
	}
}
