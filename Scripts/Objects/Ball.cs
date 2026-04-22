using Godot;
using System;

public partial class Ball : RigidBody3D
{
	[Export] public StaticBody3D Wall, Ground;
	private Score score;
	private Vector3 min, max;
	public static GpuParticles3D WallParticles;
	public AudioStreamPlayer3D BallAudio;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		score = GetNode<Score>("%Score");
		WallParticles = GetNode<GpuParticles3D>("%WallParticles");
		BallAudio = GetNode<AudioStreamPlayer3D>("BallAudio");
		BodyEntered += OnBallCollide;
		CollisionShape3D GroundShape = Ground.GetNode<CollisionShape3D>("GroundCollision");
		if(GroundShape.Shape is BoxShape3D s) {
			min = GroundShape.GlobalPosition - (s.Size / 2.0f);
			max = GroundShape.GlobalPosition + (s.Size / 2.0f);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	[Signal]
	public delegate void OnBallThrowEventHandler();
	[Signal]
	public delegate void OnBallCatchEventHandler();
	
	public void OnBodyEntered(Node node) {
		if(node.Name == "Wall" && !GameManager.HitWall) {
			if(LinearVelocity.Length() >= 60) {
				WallParticles.Restart();
				WallParticles.Emitting = true;
			}
		
		}
	}
	public void Catch(GameManager.PossessionEnum p, Vector3 pos) {
		GameManager.previous_pos = GameManager.possession;
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
		EmitSignal(SignalName.OnBallCatch);
	}
	public void Throw(GameManager.ThrowerEnum t, 
		Vector3 Direction, float Speed) {
		GameManager.previous_pos = GameManager.possession;
		GameManager.possession = GameManager.PossessionEnum.NONE;
		GameManager.thrower = t;
		GameManager.Pitches = 0;
		GameManager.HitWall = false;
		GameManager.HitFloor = false;
		
		Freeze = false;
		Sleeping = false;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		ApplyImpulse(Direction * Speed);
		EmitSignal(SignalName.OnBallThrow);
	}
	protected void Swap(ref Vector3 a, ref Vector3 b) {
		Vector3 temp = b;
		b = a;
		a = temp;
	}
	/*private GameManager.ThrowerEnum TransferEnums() {
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
	}*/
	private void OnBallCollide(Node node) {
		GameManager.CWall = (node.Name == "Wall");
		GameManager.CFloor = (node.Name == "Ground");
		if(GameManager.CWall) {
			if(!GameManager.HitWall){
				if(LinearVelocity.Length() >= 60) {
					WallParticles.Restart();
					WallParticles.Emitting = true;
				}
			}
			float lvel = LinearVelocity.Length();
			BallAudio.VolumeDb = Mathf.Remap(lvel, 0, 5, -60, 0);
			BallAudio.PitchScale = 1;
			BallAudio.Play();
			
			GameManager.Pitches = 0;
			GameManager.HitWall = true;
		}
		if(GameManager.CFloor) {
			float lvel = LinearVelocity.Length();
			BallAudio.VolumeDb = Mathf.Remap(lvel, 0, 5, -60, 0);
			BallAudio.PitchScale = 2;
			BallAudio.Play();
			
			GameManager.Pitches++;
			GameManager.HitFloor = true;
		}
	}
}
