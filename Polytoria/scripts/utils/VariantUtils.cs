// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Godot;

namespace Polytoria.Utils;

// Converts a value coming from Luau into a Godot Variant. Only the marshallable scalar/struct types are accepted;
// anything else (e.g. dictionaries that could encode a method-call key) is rejected. I am not yet comfortable
// implimenting logic that marshalls method-call, as it will require extensive testing.
//
// This can later be implemented as a PT scripting type, as it will be helpful for bridging Godot features.

public class Variant
{

	// Converts a Godot Variant to a value marshallable to Luau.
	public static object? FromGodot(Godot.Variant variant) => variant.Obj;

	// Converts a Luau object to a Godot Variant.
	public static Godot.Variant ToGodot(object? value)
	{
		return value switch
		{
			null => new Godot.Variant(),
			bool b => b,
			string s => s,
			// Luau numbers arrive as double.
			double d => d,
			// Accept the other numeric types defensively.
			float f => f,
			int i => i,
			long l => l,
			Vector2 v2 => v2,
			Vector3 v3 => v3,
			Quaternion q => q,
			Color c => c,
			_ => throw new ArgumentException($"Unsupported type for value: {value.GetType().Name}")
		};
	}
}
