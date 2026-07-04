using Godot;
using System;
using System.Linq;

public partial class NPCPlayer
{
	private void CheckForPossession(double delta)
	{
		if(Frames < 120) return;
		if(Ball.GlobalPosition.DistanceTo(GlobalPosition) <= 5f)
		{
			if(state != NPCState.RUNNING && GameManager.possession == GameManager.PossessionEnum.NONE && CanHold)
			{
				if(rng.Randf() <= 0.7f)
				{
					SetPossession((float)delta);
				}
				else {
					SetRunning();
				}
			}
		}
	}
	private void SetPossession(float delta) {
		if(NpcController.CurrentPossession == null) NpcController.CurrentPossession = this;

		state = NPCState.HOLDING;

		Vector3 BonePos = NpcSkeleton.ToGlobal(NpcSkeleton.GetBoneGlobalPose(BoneIndex).Origin);
		BallControl.Catch((TEAM == 0) ? GameManager.PossessionEnum.TEAM : GameManager.PossessionEnum.OPPONENT, 
				BonePos + new Vector3(0.0f, 0.25f, 0.0f));

		if(TEAM == 0 && NpcController.NPCS.Where(n => n.TEAM == 1 && n.state == NPCState.RUNNING).Count() > 0) Hold = Frames + (0.25f/delta);
		else if(TEAM == 1 && NpcController.NPCS.Where(n => n.TEAM == 0 && n.state == NPCState.RUNNING).Count() > 0) Hold = Frames + (0.25f/delta);
		else Hold = Frames + (rng.Randf() * Max_Hold) / (delta * 60f) + (0.4f / delta);
	}
	private void ThrowBall() {
		//AnimController.PlayThrow();
		Hold = -1;
		float DirectionX = rng.RandfRange(WallMin.X, WallMax.X);
		float DirectionY = rng.RandfRange(GlobalPosition.Y + 5.0f, WallMax.Y - 5.0f);
		float DirectionZ = WallMax.Z;
		Vector3 Point = new Vector3(DirectionX, DirectionY, DirectionZ);
		Vector3 Direction = (Point - GlobalPosition).Normalized();
		float Speed = rng.RandfRange(Throw_Speed, Throw_Max);
		BallControl.Throw((TEAM == 0) ? GameManager.ThrowerEnum.TEAM : GameManager.ThrowerEnum.OPPONENT, 
			Direction, Speed);
		throwFrame = Frames;
		if(NpcController.CurrentPossession == this) NpcController.CurrentPossession = null;
		state = NPCState.AT_TARGET;
		SetTarget();
	}
	private void HoldBall() {
		if(Frames >= Hold && Hold != -1) {
			state = NPCState.THROWING;
			return;
		}
		Ball.GlobalPosition = GlobalPosition + new Vector3(0.0f, 2.0f, 1.0f);
		if(GlobalPosition.Z > SafePoint.GlobalPosition.Z) {
			SetTarget();
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
