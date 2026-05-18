// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// UIView is a class that displays a rectangle in your place's UI.
/// </summary>
[Instantiable]
public partial class UIView : UIField
{
	private StyleBoxFlat _styleBox = null!;
	private Color _borderColor;
	private Color _color;
	private float _borderWidth;
	private float _cornerRadius;


	/// <summary>
	/// Determines the color of the UI.
	/// </summary>
	/// <summary>
	/// Determines the border color of the UI.
	/// </summary>
	[Editable, ScriptProperty]
	public Color BorderColor
	{
		get => _borderColor;
		set
		{
			_borderColor = value;
			_styleBox.BorderColor = _borderColor;
			OnPropertyChanged();
		}
	}

	[Editable, ScriptProperty]
	public Color Color
	{
		get => _color;
		set
		{
			_color = value;
			_styleBox.BgColor = _color;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the width of the border of the UI.
	/// </summary>
	[Editable, ScriptProperty]
	public float BorderWidth
	{
		get => _borderWidth;
		set
		{
			_borderWidth = value;
			if (_borderWidth > 0 && BorderColor.A == 0)
			{
				_borderWidth = 0;
			}
			_styleBox.BorderWidthTop = (int)_borderWidth;
			_styleBox.BorderWidthBottom = (int)_borderWidth;
			_styleBox.BorderWidthLeft = (int)_borderWidth;
			_styleBox.BorderWidthRight = (int)_borderWidth;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Determines the corner radius of the UI.
	/// </summary>
	[Editable, ScriptProperty]
	public float CornerRadius
	{
		get => _cornerRadius;
		set
		{
			_cornerRadius = value;
			_styleBox.CornerRadiusTopLeft = (int)value;
			_styleBox.CornerRadiusTopRight = (int)value;
			_styleBox.CornerRadiusBottomLeft = (int)value;
			_styleBox.CornerRadiusBottomRight = (int)value;
			OnPropertyChanged();
		}
	}

	public override void Init()
	{
		_styleBox = new() { AntiAliasing = true, AntiAliasingSize = 1 };
		NodeControl.AddThemeStyleboxOverride("panel", _styleBox);
		BorderColor = new(0, 0, 0);
		Color = new(1, 1, 1);
		BorderWidth = 0;
		CornerRadius = 0;
		base.Init();
	}

}
