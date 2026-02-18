using Godot;
using System;

public partial class Ball : RigidBody3D
{
	[Export] public StaticBody3D Wall, Ground;
	private Score score;
	private Vector3 min, max;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		score = GetNode<Score>("/root/Root/Score");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	private void OnBodyEntered(Node3D node) {
		
	}
	public void Catch(GameManager.PossessionEnum p, Vector3 pos) {
		GameManager.possession = p;
		GameManager.previous = GameManager.thrower;
		GameManager.thrower = GameManager.ThrowerEnum.NONE;
		if(GameManager.Pitches == 0) {
			score.CatchScore();
		}
		Freeze = true;
		/*GlobalTransform = new Transform3D(Basis.Identity, 
			cam.GlobalPosition - cam.Rotation);*/
		GlobalPosition = pos;
	}
	public void Throw(GameManager.ThrowerEnum t, 
		Vector3 Direction, float Speed) {
		if(t == GameManager.ThrowerEnum.RUNNING) GameManager.previous = TransferEnums();
		else if(t != GameManager.ThrowerEnum.RUNNING) GameManager.previous = GameManager.thrower; 
		GameManager.possession = GameManager.PossessionEnum.NONE;
		GameManager.thrower = t;
		GameManager.Pitches = 0;
		
		Freeze = false;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		ApplyImpulse(Direction * Speed);
	}
	protected void Swap(ref Vector3 a, ref Vector3 b) {
		Vector3 temp = b;
		b = a;
		a = temp;
	}
	private GameManager.ThrowerEnum TransferEnums() {
		GameManager.ThrowerEnum result;
		if(GameManager.possession == GameManager.PossessionEnum.PLAYER) {
			result =  GameManager.ThrowerEnum.PLAYER;
		} else if(GameManager.possession == GameManager.PossessionEnum.TEAM) {
			result =  GameManager.ThrowerEnum.TEAM;
		} else if(GameManager.possession == GameManager.PossessionEnum.OPPONENT) {
			result =  GameManager.ThrowerEnum.OPPONENT;
		} else {
			result =  GameManager.thrower;
		}
		GD.Print("Enum Transferred from: " + GameManager.thrower + " To: " + result);
		return result;
	}
}
