// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Datamodel.Interfaces;
using Polytoria.Scripting;
using System;

namespace Polytoria.Datamodel.Data;

/// <summary>
/// NumberRange is a data type that represents a range between two numbers, defined by a minimum and maximum value.
/// </summary>
public struct NumberRange : IScriptObject, IData
{
	private float _min = 0;
	private float _max = 0;

	/// <summary>
	/// Determines the minimum value of the range.
	/// </summary>
	[ScriptProperty]
	public float Min
	{
		readonly get => _min;
		set
		{
			_min = value;
			if (_min > _max)
			{
				_max = _min;
			}
		}
	}

	/// <summary>
	/// Determines the maximum value of the range.
	/// </summary>
	[ScriptProperty]
	public float Max
	{
		readonly get => _max;
		set
		{
			_max = value;
			if (_max < _min)
			{
				_min = _max;
			}
		}
	}

	public NumberRange() { }

	/// <summary>
	/// Creates a new NumberRange object with the specified minimum and maximum values.
	/// </summary>
	[ScriptMethod]
	public static NumberRange New(float from, float to)
	{
		return new() { Min = from, Max = to };
	}

	/// <summary>
	/// Linearly interpolates between the minimum and maximum values of the range based on the parameter t, which is typically between 0 and 1.
	/// </summary>
	[ScriptMethod]
	public readonly float Lerp(float t)
	{
		return Mathf.Lerp(Min, Max, t);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(Min, Max);
	}

	public object Clone()
	{
		return new NumberRange() { Min = Min, Max = Max };
	}
}
