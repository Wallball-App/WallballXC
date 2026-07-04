using Godot;
using System;

public partial class NPCPlayer
{
	private void SetMaterial() {
		if(state == NPCState.RUNNING) {
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
		} else if(state == NPCState.HOLDING){
			Triangle.Visible = true;
			RecursiveSearch(Triangle, Green);
		} else {
			Triangle.Visible = false;
		}
		
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
		Possession_Forward = GameManager.possession;
		Thrower_Forward = GameManager.thrower;
		Possession0 = (GameManager.possession == GameManager.PossessionEnum.NONE);
		HitWall0 = GameManager.HitWall;
		CanHold = (Frames > throwFrame + ThrowCooldown/(float)delta) && (GameManager.HitWall || (NpcController.RunningCount >= 1));
	}
	private void CheckForRunning()
	{
		if(Frames >= RunFrame && RunFrame != 0) SetRunning();
	}
	private void SetRunning()
	{
		state = NPCState.RUNNING;
		BallControl.Throw((TEAM == 0) ? GameManager.ThrowerEnum.TEAM : GameManager.ThrowerEnum.OPPONENT, 
					Vector3.Down, 1);
		Target = CreateNewTarget(GameManager.HitWall, state);
		TargetFrame = Frames;
	}
	private void UpdateAnimationTreeConditions()
	{
		
		Vector3 modifiedGP = GlobalPosition + new Vector3(0.0f, 2.25f, 0.0f);
		Vector3 offset = GlobalTransform.Inverse() * Target;

		Vector3 dir = modifiedGP.DirectionTo(offset);
		Vector2 relativePositionVector = new Vector2(dir.X, dir.Y);
		relativePositionVector = relativePositionVector.Clamp(-1.0f, 1.0f);

		NPCAnimationTree.Set("parameters/MainStateMachine/NPC_Catch_Blend/blend_position", relativePositionVector);
		NPCAnimationTree.Set("parameters/MainStateMachine/conditions/IsJumping", IsJumping);

		CatchAnimation = IsChasingBall && (pos2d.DistanceTo(target2d) <= Velocity.Length()*0.5f) || 
			(pos2d.DistanceTo(target2d) <= 5f && Ball.GlobalPosition.Y > GlobalPosition.Y + 4) && Possession0;
		NPCAnimationTree.Set("parameters/MainStateMachine/conditions/Catch", CatchAnimation);

		NPCAnimationTree.Set("parameters/MainStateMachine/conditions/Throw", state == NPCState.THROWING);
	}
}
