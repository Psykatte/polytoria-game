// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// GradientSky is a class that is used to set a gradient skybox in the world.
/// </summary>
[Instantiable]
[DocCategory("lighting")]
public sealed partial class GradientSky : Sky
{
	private ShaderMaterial _mat = null!;

	private Color _sunDiscColor = new(1, 1, 1, 1);
	private float _sunDiscMultiplier = 25;
	private float _sunDiscExponent = 125000;

	private Color _sunHaloColor = new(0.8970588f, 0.7760561f, 0.6661981f, 1);
	private float _sunHaloExponent = 125;
	private float _sunHaloContribution = 0.75f;

	private Color _horizonLineColor = new(0.9044118f, 0.8872592f, 0.7913603f, 1);
	private float _horizonLineExponent = 4;
	private float _horizonLineContribution = 0.25f;

	private Color _skyGradientTop = new(0.172549f, 0.5686274f, 0.6941177f, 1);
	private Color _skyGradientBottom = new(0.764706f, 0.8156863f, 0.8509805f);
	private float _skyGradientExponent = 2.5f;

	/// <summary>
	/// Determines the color emitting off the sun.
	/// </summary>
	[Editable, ScriptProperty]
	public Color SunDiscColor
	{
		get => _sunDiscColor;
		set
		{
			_sunDiscColor = value;
			_mat.SetShaderParameter("sun_disc_color", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the multiplier of the sun.
	/// </summary>
	[Editable, ScriptProperty]
	public float SunDiscMultiplier
	{
		get => _sunDiscMultiplier;
		set
		{
			_sunDiscMultiplier = value;
			_mat.SetShaderParameter("sun_disc_multiplier", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the exponent of the sun.
	/// </summary>
	[Editable, ScriptProperty]
	public float SunDiscExponent
	{
		get => _sunDiscExponent;
		set
		{
			_sunDiscExponent = value;
			_mat.SetShaderParameter("sun_disc_exponent", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the color of the sun halo.
	/// </summary>
	[Editable, ScriptProperty]
	public Color SunHaloColor
	{
		get => _sunHaloColor;
		set
		{
			_sunHaloColor = value;
			_mat.SetShaderParameter("sun_halo_color", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the exponent of the sun halo.
	/// </summary>
	[Editable, ScriptProperty]
	public float SunHaloExponent
	{
		get => _sunHaloExponent;
		set
		{
			_sunHaloExponent = value;
			_mat.SetShaderParameter("sun_halo_exponent", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the contribution of the sun halo.
	/// </summary>
	[Editable, ScriptProperty]
	public float SunHaloContribution
	{
		get => _sunHaloContribution;
		set
		{
			_sunHaloContribution = value;
			_mat.SetShaderParameter("sun_halo_contribution", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the horizon line's color.
	/// </summary>
	[Editable, ScriptProperty]
	public Color HorizonLineColor
	{
		get => _horizonLineColor;
		set
		{
			_horizonLineColor = value;
			_mat.SetShaderParameter("horizon_line_color", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the horizon line's exponent.
	/// </summary>
	[Editable, ScriptProperty]
	public float HorizonLineExponent
	{
		get => _horizonLineExponent;
		set
		{
			_horizonLineExponent = value;
			_mat.SetShaderParameter("horizon_line_exponent", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines how much the horizon line contributes.
	/// </summary>
	[Editable, ScriptProperty]
	public float HorizonLineContribution
	{
		get => _horizonLineContribution;
		set
		{
			_horizonLineContribution = value;
			_mat.SetShaderParameter("horizon_line_contribution", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the top color of the gradient.
	/// </summary>
	[Editable, ScriptProperty]
	public Color SkyGradientTop
	{
		get => _skyGradientTop;
		set
		{
			_skyGradientTop = value;
			_mat.SetShaderParameter("sky_gradient_top", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the bottom color of the gradient.
	/// </summary>
	[Editable, ScriptProperty]
	public Color SkyGradientBottom
	{
		get => _skyGradientBottom;
		set
		{
			_skyGradientBottom = value;
			_mat.SetShaderParameter("sky_gradient_bottom", value);
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the gradient's exponent.
	/// </summary>
	[Editable, ScriptProperty]
	public float SkyGradientExponent
	{
		get => _skyGradientExponent;
		set
		{
			_skyGradientExponent = value;
			_mat.SetShaderParameter("sky_gradient_exponent", value);
			OnPropertyChanged();
		}
	}

	public override void Init()
	{
		_mat = new()
		{
			Shader = GD.Load<Shader>("res://resources/shaders/skybox.gdshader")
		};
		SkyMaterial = _mat;

		base.Init();
	}
}
