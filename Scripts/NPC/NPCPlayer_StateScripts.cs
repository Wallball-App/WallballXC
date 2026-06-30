using Godot;
using System;

public partial class NPCPlayer
{
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
}
