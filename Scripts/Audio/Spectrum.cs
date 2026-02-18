using Godot;
using System;

public partial class Spectrum : ProgressBar
{
	[Export] public float MinFrequency = 20;
	[Export] public float MaxFrequency = 440;
	[Export] public float Multiplier = 1000;
	private AudioEffectSpectrumAnalyzerInstance _spectrum;

	public override void _Ready()
	{
		_spectrum = (AudioEffectSpectrumAnalyzerInstance)AudioServer.GetBusEffectInstance(0, 0);
	}

	public override void _Process(double delta)
	{
		Vector2 m = _spectrum.GetMagnitudeForFrequencyRange(MinFrequency, MaxFrequency);
		Value = Mathf.Lerp((float)Value, 
					m.Length() * Multiplier,
					(float)delta * 10.0f);
	}
}
