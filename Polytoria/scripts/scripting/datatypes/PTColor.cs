// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Scripting.Datatypes;

/// <summary>
/// Color is a data type that represents a color. The alpha property is between 0 and 1. 0 is fully transparent and 1 is fully visible.
/// </summary>
[DocCategory("types")]
public class PTColor : IScriptGDObject
{
	Color color;

	/// <summary>
	/// Red color component.
	/// </summary>
	[ScriptProperty] public float R { get => color.R; set => color.R = value; }

	/// <summary>
	/// Green color component.
	/// </summary>
	[ScriptProperty] public float G { get => color.G; set => color.G = value; }

	/// <summary>
	/// Blue color component.
	/// </summary>
	[ScriptProperty] public float B { get => color.B; set => color.B = value; }

	/// <summary>
	/// Alpha (opacity) color component.
	/// </summary>
	[ScriptProperty] public float A { get => color.A; set => color.A = value; }

	public static PTColor FromGDClass(Color clr)
	{
		return new PTColor()
		{
			color = clr
		};
	}

	public object ToGDClass()
	{
		return color;
	}

	/// <summary>
	/// Creates a new Color with the set R, G, B and A values.
	/// </summary>
	[ScriptMethod]
	public static PTColor New()
	{
		return new()
		{
			R = 0,
			G = 0,
			B = 0,
			A = 1
		};
	}

	/// <summary>
	/// Creates a new Color with the set R, G, B and A values.
	/// </summary>
	[ScriptMethod]
	public static PTColor New(float d)
	{
		return new()
		{
			R = d,
			G = d,
			B = d,
			A = 1
		};
	}

	/// <summary>
	/// Creates a new Color with the set R, G, B and A values.
	/// </summary>
	[ScriptMethod]
	public static PTColor New(float r, float g, float b)
	{
		return new()
		{
			R = r,
			G = g,
			B = b,
			A = 1
		};
	}

	/// <summary>
	/// Creates a new Color with the set R, G, B and A values.
	/// </summary>
	[ScriptMethod]
	public static PTColor New(float r, float g, float b, float a)
	{
		return new()
		{
			R = r,
			G = g,
			B = b,
			A = a
		};
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Add)]
	public static PTColor Add(PTColor a, PTColor b)
	{
		return FromGDClass(a.color + b.color);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Sub)]
	public static PTColor Sub(PTColor a, PTColor b)
	{
		return FromGDClass(a.color - b.color);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static object Mul(PTColor a, object b)
	{
		if (b is double d)
			return FromGDClass(a.color * new Color((float)d, (float)d, (float)d));
		return FromGDClass(a.color);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Eq)]
	public static bool Eq(PTColor a, PTColor b)
	{
		return a.color == b.color;
	}

	[ScriptMetamethod(ScriptObjectMetamethod.ToString)]
	public static string ToString(PTColor? v)
	{
		if (v == null) return "<Color>";
		return $"<Color:({v.color.R}, {v.color.G}, {v.color.B}, {v.color.A}>";
	}

	/// <summary>
	/// Returns a random color with an alpha value of 1.
	/// </summary>
	[ScriptMethod]
	public static PTColor Random()
	{
		return New(GD.Randf(), GD.Randf(), GD.Randf());
	}

	/// <summary>
	/// Creates a new Color from the specified RGBA value.
	/// </summary>
	[ScriptMethod]
	public static PTColor FromRGB(float r, float g, float b, float a = 1)
	{
		return FromGDClass(new Color(r / 255, g / 255, b / 255, a));
	}

	/// <summary>
	/// Creates a new Color from the specified hex value.
	/// </summary>
	[ScriptMethod]
	public static PTColor FromHex(string hex)
	{
		return FromGDClass(Color.FromString(hex, new(1, 1, 1)));
	}

	/// <summary>
	/// Converts a Color value to its hexadecimal string representation.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static string ToHex(PTColor c)
	{
		return c.color.ToHtml();
	}

	/// <summary>
	/// Creates a new Color from the specified HSV value.
	/// </summary>
	[ScriptMethod]
	public static PTColor FromHSV(float h, float s, float v, float a = 1)
	{
		return FromGDClass(Color.FromHsv(h, s, v, a));
	}

	/// <summary>
	/// Linearly interpolates colors a and b by t.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTColor Lerp(PTColor a, PTColor b, float t)
	{
		return FromGDClass(a.color.Lerp(b.color, t));
	}
}
