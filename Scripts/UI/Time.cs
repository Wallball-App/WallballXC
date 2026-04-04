using Godot;
using System;

public partial class Time : MenuButton
{
	private Label TimeLabel;
	private HSlider TimeSlider;
	private ColorPicker TeamColor, OpponentColor;
	
	private double Hour, Minute;
	private string AMPM;

	private Node Game;
	private Node SkyController;
	private Node TimeController;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TimeLabel = GetNode<Label>("%TimeLabel");
		TimeSlider = GetNode<HSlider>("%TimeSlider");
		
		TeamColor = GetNode<ColorPicker>("%TeamColor");
		OpponentColor = GetNode<ColorPicker>("%OpponentColor");
		
		Game = ResourceLoader.Load<PackedScene>("res://Scenes/Maps/Game.tscn").Instantiate<Node>();
		SkyController = Game.GetNode<Node>("%Sky3D");
		TimeController = SkyController.GetNode<Node>("TimeOfDay");
		//var SkyDomeController = SkyController.GetNode<SkyDome>("SkyDome");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Hour = Math.Floor(TimeSlider.Value);
		Minute = Math.Round((TimeSlider.Value - Hour) * 60);
		string MinuteFinal = (Minute < 10) ? "0" + Minute.ToString() : Minute.ToString();
		AMPM = (Hour / 12 < 1) ? "AM" : "PM";
		TimeLabel.Text = "Time of Day: " + Hour.ToString() + ":" + MinuteFinal + " " + AMPM;
		
		TimeController.Set("current_time", TimeSlider.Value);
	}
}
