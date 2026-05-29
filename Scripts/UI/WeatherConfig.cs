using Godot;
using GodotCookies;
using System;

public partial class WeatherConfig : PanelContainer
{
	private HSlider TimeSlider;
	private Label TimeLabel;
	private OptionButton CloudPresets;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TimeSlider = GetNode<HSlider>("%TimeSlider");
		TimeLabel = GetNode<Label>("%TimeLabel");
		CloudPresets = GetNode<OptionButton>("%CloudPresets");

		TimeSlider.Value = Cookies.User.Get<float>("Time");
		CloudPresets.Selected = Cookies.User.Get<int>("CloudPreset");

		if(TimeSlider.Value == 0) TimeSlider.Value = 16.0f;
		if(CloudPresets.Selected == -1) CloudPresets.Selected = 1;

		TimeSlider.ValueChanged += TimeSlider_ValueChanged;
		CloudPresets.ItemSelected += CloudPresets_ItemSelected;

		TreeExiting += SaveAll;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void TimeSlider_ValueChanged(double value)
	{
		Cookies.User.Set("Time", value);
	}
	private void CloudPresets_ItemSelected(long index)
	{
		Cookies.User.Set("CloudPreset", index);
	}
	private void SaveAll()
	{
		//if(TimeSlider.Value == 0) TimeSlider.Value = 12.0f;
		if(CloudPresets.Selected == -1) CloudPresets.Selected = 1;

		Cookies.User.Set("Timer", TimeSlider.Value);
		Cookies.User.Set("CloudPreset", CloudPresets.Selected);

		GD.Print("Time Set To: " + TimeSlider.Value);
		GD.Print("Cloud Preset Set To: " + CloudPresets.Selected);
	}
}
