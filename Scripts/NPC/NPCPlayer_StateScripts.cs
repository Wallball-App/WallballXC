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

		CatchAnimation = IsChasingBall && (pos2d.DistanceTo(target2d) <= 5) || 
			(pos2d.DistanceTo(target2d) <= 5 && Ball.GlobalPosition.Y > GlobalPosition.Y + 4) && Possession0 && HitWall0;
		NPCAnimationTree.Set("parameters/MainStateMachine/conditions/Catch", CatchAnimation);
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
}
