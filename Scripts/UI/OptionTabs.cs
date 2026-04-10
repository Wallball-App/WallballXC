using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class OptionTabs : VBoxContainer
{
	private List<Button> Buttons;
	private List<Control> Panels;
	private OnLoad Alpha;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Buttons = GetChildren().Cast<Button>().ToList();
		Panels = GetNode<Panel>("%View").GetChildren().Cast<Control>().ToList();
		foreach(Control child in Panels) {
			child.Visible = false;
		}
		foreach(Button child in Buttons) {
			child.Pressed += () => OnButtonPress(Buttons.IndexOf(child));
		}
		OnButtonPress(0);
		Alpha = GetNode<OnLoad>("%Alpha");
		Alpha.FadeIn();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void OnButtonPress(int index) {
		foreach(Control child in Panels) {
			child.Visible = (Panels.IndexOf(child) == index);
		}
	}
}
