using Godot;
using System;
using System.Linq;

public partial class NPCPlayer
{
	private void MoveTowardTarget(double delta) {
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
		Vector3 steering = (desired - velocity) * (WalkSpeed/BaseSpeed) * 3; //Steering = desired - current, with weight (3/time)
		steering = steering.LimitLength(WalkSpeed * Math.Clamp(Dist, 0.0f, 0.75f)); //Limit steering to prevent overshooting, with distance-based scaling
		
		
		if(IsOnWall()) steering = steering - (0.5f * steering);


		Velocity += steering * (float)delta;
		Velocity = new Vector3(Velocity.X, velocity.Y, Velocity.Z); //Y is based on modified Velocity, while X and Z are steering

		if(Velocity.Length() >= 0.5f) { //Slerp Rotation to watch Target
			Basis lookat;
			if(state == NPCState.HOLDING) {
				Vector3 wallDir = Wall.GlobalPosition.Normalized();
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
		
		Triangle.GlobalPosition = GlobalPosition + new Vector3(0.0f, 5.0f, 0.0f);

		if(Frames > TargetFrame + ResetTime/(float)delta) SetTarget();

		/*float calculated_scale = pos2d.DistanceTo(target2d)/Velocity.Length();
		if(!float.IsFinite(calculated_scale)) calculated_scale = 1.0f;
		float speedscale = (CatchAnimation) ?  calculated_scale : 1.0f;
		NPCAnimationTree.Set("parameters/TimeScale/scale", speedscale);*/
	}
	private void ApplyGravity(double delta) {
		if(!IsOnFloor() || IsJumping) {
			Vector3 velocity = Velocity;
			velocity.Y -= Gravity * (float)delta;
			Velocity = velocity;
		}
	}
}
