// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// ColorAdjustModifier is a LightingModifier that allows the adjustment of lighting
/// </summary>
[Instantiable]
[DocCategory("elements")]
public partial class ColorAdjustModifier : LightingModifier
{
	private float _brightness;
	private float _contrast;
	private float _saturation;
	private Color _tintColor = new(1, 1, 1);
	private GradientTexture1D _tintColorGradient = null!;

	/// <summary>
	/// Determines the brightness adjustment.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(1f)]
	public float Brightness
	{
		get => _brightness;
		set
		{
			_brightness = value;
			ApplyEffects();
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the contrast adjustment.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(1f)]
	public float Contrast
	{
		get => _contrast;
		set
		{
			_contrast = value;
			ApplyEffects();
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the saturation adjustment.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(1f)]
	public float Saturation
	{
		get => _saturation;
		set
		{
			_saturation = value;
			ApplyEffects();
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the tint color.
	/// </summary>
	[Editable, ScriptProperty]
	public Color TintColor
	{
		get => _tintColor;
		set
		{
			_tintColor = value;
			ApplyEffects();
			OnPropertyChanged();
		}
	}

	public override void Init()
	{
		base.Init();
		_tintColorGradient = new()
		{
			Gradient = new()
		};
		ApplyEffects();
	}

	public override void PreDelete()
	{
		Root.Lighting.environment.AdjustmentEnabled = false;
		base.PreDelete();
	}

	private void ApplyEffects()
	{
		if (IsHidden)
		{
			Root.Lighting.environment.AdjustmentEnabled = false;
			return;
		}
		_tintColorGradient.Gradient.SetColor(1, _tintColor);
		Root.Lighting.environment.AdjustmentEnabled = true;
		Root.Lighting.environment.AdjustmentBrightness = _brightness;
		Root.Lighting.environment.AdjustmentContrast = _contrast;
		Root.Lighting.environment.AdjustmentSaturation = _saturation;
		Root.Lighting.environment.AdjustmentColorCorrection = _tintColorGradient;
	}

	public override void HiddenChanged(bool to)
	{
		ApplyEffects();
		base.HiddenChanged(to);
	}
}
