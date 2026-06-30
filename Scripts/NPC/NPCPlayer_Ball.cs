using Godot;
using System;
using System.Linq;

public partial class NPCPlayer
{
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
