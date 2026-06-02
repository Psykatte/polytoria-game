// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Scripting.Extensions;

public class LuaExtensionMath : IScriptObject
{
	/// <summary>
	/// Returns the inverse linear interpolation factor of <paramref name="weight"/> between <paramref name="a"/> and <paramref name="b"/>.
	/// </summary>
	[ScriptMethod("invlerp")]
	public static double InvLerp(double a, double b, double weight)
	{
		return Mathf.InverseLerp(a, b, weight);
	}

	/// <summary>
	/// Remaps <paramref name="value"/> from the input range [<paramref name="inFrom"/>, <paramref name="inTo"/>] to the output range [<paramref name="outFrom"/>, <paramref name="outTo"/>] and returns the result.
	/// </summary>
	[ScriptMethod("remap")]
	public static double Remap(double value, double inFrom, double inTo, double outFrom, double outTo)
	{
		return Mathf.Remap(value, inFrom, inTo, outFrom, outTo);
	}
}
