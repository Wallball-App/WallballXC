using Godot;
using System;
using System.Linq;

public partial class NPCPlayer
{
    private void SetTarget()
	{
		ResetTime = rng.RandfRange(1.0f, 3.0f); //SET RANDOM RESET

		if(state == NPCState.RUNNING)
		{
			Target = CreateNewTarget(GameManager.HitWall, state);
			TargetFrame = Frames; //Target-Change Frame Set
			if(state == NPCState.AT_TARGET) state = NPCState.MOVE_TOWARD_TARGET;
			return;
		}
		
		if(state != NPCState.RUNNING && GlobalPosition.DistanceTo(Target) >= 32.0f) //Set Idle Animation
		{
			if(rng.Randf() <= 0.2f) //Prevent Freezing 
			{
				Target = CreateNewTarget(GameManager.HitWall, state);
				TargetFrame = Frames; //Target-Change Frame Set
				if(state == NPCState.AT_TARGET) state = NPCState.MOVE_TOWARD_TARGET;
				return;
			}
			Target = GlobalPosition;
			if(IsChasingBall) IsChasingBall = false;
			Velocity = new Vector3(0.0f, Velocity.Y, 0.0f); //To Account for Gravity
			Vector3 ballPos = new Vector3(Ball.GlobalPosition.X, GlobalPosition.Y, Ball.GlobalPosition.Z);
			Basis lookat = Basis.LookingAt(ballPos, Vector3.Up);
			Basis = Basis.Orthonormalized().Slerp(lookat.Orthonormalized(), 0.5f);

			TargetFrame = Frames; //Target-Change Frame Set
			if(state == NPCState.AT_TARGET) state = NPCState.MOVE_TOWARD_TARGET;
			return;
		}
		if(GameManager.possession == GameManager.PossessionEnum.NONE && !GameManager.HitWall) { //PREDICT TRAJECTORY OF BALL
			if(state != NPCState.RUNNING) Target = PredictTrajectory();
			IsChasingBall = true;
			Sprint = Dist/WorldSize + 1f;
			WalkSpeed = BaseSpeed * Sprint;

			TargetFrame = Frames; //Target-Change Frame Set
			if(state == NPCState.AT_TARGET) state = NPCState.MOVE_TOWARD_TARGET;
			return;
		}
		if(rng.Randf() <= 0.75f) //IDLE OR TARGET LOGIC
		{
			Target = CreateNewTarget(GameManager.HitWall, state);
			TargetFrame = Frames;
			if(state == NPCState.AT_TARGET) state = NPCState.MOVE_TOWARD_TARGET;
			return;
		}
		if(state == NPCState.AT_TARGET) state = NPCState.MOVE_TOWARD_TARGET;
		TargetFrame = Frames; //Target-Change Frame Set
	}
	private Vector3 CreateNewTarget(bool Wall, NPCState state) {
		if(state == NPCState.RUNNING) return SafePoint.GlobalPosition + new Vector3(0f, 0f, 2f);
		else if(Wall || (NpcController.NPCS.Where(n => n.state == NPCState.RUNNING).Count() > 0 &&
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
	private void CheckForTargetReset()
	{
		if(Frames >= TargetFrame + (int)(ResetTime/delta)) {
			SetTarget();
			ResetTime = rng.RandfRange(1.0f, 3.0f); //SET RANDOM RESET
		}
		if(GlobalPosition.DistanceTo(Target) <= 5.0f)
		{
			if(state == NPCState.MOVE_TOWARD_TARGET) state = NPCState.AT_TARGET;
		}
	}
	private void CheckWallCollide() {
		if(GlobalPosition.Z > SafePoint.GlobalPosition.Z) {
			if(state == NPCState.RUNNING) state = NPCState.AT_TARGET;
			Target = CreateNewTarget(GameManager.HitWall, state); //Change HitWall to FALSE to change NPC Behavior
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
}