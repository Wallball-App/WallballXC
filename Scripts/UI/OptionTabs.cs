using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCookies;

public partial class OptionTabs : VBoxContainer
{
	private List<Button> Buttons;
	private List<Control> Panels;
	[Export] public LineEdit UserName;
	private OnLoad Alpha;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Buttons = GetChildren().Cast<Button>().ToList();
		Panels = GetNode<Panel>("%View").GetChildren().Cast<Control>().ToList();
		if(UserName != null) UserName.TextChanged += SaveUserName;
		if(UserName != null) UserName.Text = Cookies.User.Get<string>("UserName") ?? "";

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
	private void SaveUserName(string text)
	{
		Cookies.User.Set("UserName", text);
	}
}
