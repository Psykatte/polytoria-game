// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using MemoryPack;
using Polytoria.Attributes;
using Polytoria.Datamodel.Interfaces;
using Polytoria.Enums;
using Polytoria.Scripting;

namespace Polytoria.Datamodel.Data;

/// <summary>
/// A single drop-shadow layer; color, offset, blur radius, and spread; as used by UIShadow.
/// </summary>
[MemoryPackable]
[DocCategory("effects")]
public partial struct ShadowLayer : IScriptObject, IData
{
	private float _radius;

	/// <summary>
	/// The color of the shadow, including its opacity.
	/// </summary>
	[MemoryPackAllowSerialize]
	[ScriptProperty]
	public Color Color { get; set; }

	/// <summary>
	/// The horizontal and vertical offset of the shadow from its element.
	/// </summary>
	[MemoryPackAllowSerialize]
	[ScriptProperty]
	public Vector2 Offset { get; set; }

	/// <summary>
	/// The blur radius of the shadow; larger values produce a softer, more spread out shadow.
	/// </summary>
	[ScriptProperty]
	public float Radius
	{
		get => _radius;
		set => _radius = Mathf.Max(0, value);
	}

	private float _spread;

	/// <summary>
	/// The amount by which the shadow is expanded outward before blurring.
	/// </summary>
	[ScriptProperty]
	public float Spread
	{
		get => _spread;
		set => _spread = Mathf.Max(0, value);
	}

	/// <summary>
	/// The blend mode used when compositing this shadow layer onto the element.
	/// </summary>
	[ScriptProperty]
	public BlendModeEnum BlendMode { get; set; }

	public ShadowLayer()
	{
		Color = new Color(0, 0, 0, 0.2f);
		Offset = new Vector2(0, 4);
		_radius = 8f;
		Spread = 0f;
		BlendMode = BlendModeEnum.Mix;
	}

	[ScriptMethod]
	public static ShadowLayer New()
	{
		return new ShadowLayer();
	}

	object IData.Clone() => new ShadowLayer
	{
		Color = Color,
		Offset = Offset,
		Radius = Radius,
		Spread = Spread,
		BlendMode = BlendMode,
	};
}
