// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using System;

namespace Polytoria.Scripting.Datatypes;

/// <summary>
/// Vector2 is a 2D vector with an x and y component.
/// </summary>
[DocCategory("types")]
public class PTVector2 : IScriptGDObject
{
	Vector2 vector;

	/// <summary>
	/// The X component of the vector.
	/// </summary>
	[ScriptProperty] public float X { get => vector.X; set => vector.X = value; }

	/// <summary>
	/// The Y component of the vector.
	/// </summary>
	[ScriptProperty] public float Y { get => vector.Y; set => vector.Y = value; }

	/// <summary>
	/// Shorthand for Vector2.New(0, -1).
	/// </summary>
	[ScriptProperty] public static PTVector2 Down { get; private set; } = new() { X = 0, Y = -1 };

	/// <summary>
	/// Shorthand for Vector2.New(-1, 0).
	/// </summary>
	[ScriptProperty] public static PTVector2 Left { get; private set; } = new() { X = -1, Y = 0 };

	/// <summary>
	/// Shorthand for Vector2.New(1, 1).
	/// </summary>
	[ScriptProperty] public static PTVector2 One { get; private set; } = new() { X = 1, Y = 1 };

	/// <summary>
	/// Shorthand for Vector2.New(0, 0).
	/// </summary>
	[ScriptProperty] public static PTVector2 Zero { get; private set; } = new() { X = 0, Y = 0 };

	/// <summary>
	/// Shorthand for Vector2.New(1, 0).
	/// </summary>
	[ScriptProperty] public static PTVector2 Right { get; private set; } = new() { X = 1, Y = 0 };

	/// <summary>
	/// Shorthand for Vector2.New(0, 1).
	/// </summary>
	[ScriptProperty] public static PTVector2 Up { get; private set; } = new() { X = 0, Y = 1 };

	/// <summary>
	/// The length of the vector.
	/// </summary>
	[ScriptProperty] public float Magnitude => vector.Length();

	/// <summary>
	/// The normalized version of the vector.
	/// </summary>
	[ScriptProperty] public PTVector2 Normalized => FromGDClass(vector.Normalized());

	/// <summary>
	/// The squared length of the vector.
	/// </summary>
	[ScriptProperty] public float SqrMagnitude => vector.LengthSquared();

	public static PTVector2 FromGDClass(Vector2 vec)
	{
		return new PTVector2()
		{
			vector = (Vector2)vec
		};
	}

	public object ToGDClass()
	{
		return vector;
	}

	/// <summary>
	/// Returns a new Vector2 with the given x and y components.
	/// </summary>
	[ScriptMethod]
	public static PTVector2 New()
	{
		return new()
		{
			X = 0,
			Y = 0,
		};
	}

	/// <summary>
	/// Returns a new Vector2 with the given x and y components.
	/// </summary>
	[ScriptMethod]
	public static PTVector2 New(float d)
	{
		return new()
		{
			X = d,
			Y = d,
		};
	}

	/// <summary>
	/// Returns a new Vector2 with the given x and y components.
	/// </summary>
	[ScriptMethod]
	public static PTVector2 New(float x, float y)
	{
		return new()
		{
			X = x,
			Y = y,
		};
	}

	/// <summary>
	/// Adds two vectors together.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Add)]
	public static PTVector2 Add(PTVector2 a, PTVector2 b)
	{
		return FromGDClass(a.vector + b.vector);
	}

	/// <summary>
	/// Subtracts one vector from another.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Sub)]
	public static PTVector2 Sub(PTVector2 a, PTVector2 b)
	{
		return FromGDClass(a.vector - b.vector);
	}

	/// <summary>
	/// Multiplies two vectors component-wise.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector2 MulVectorVector(PTVector2 a, PTVector2 b)
		=> FromGDClass(a.vector * b.vector);

	/// <summary>
	/// Multiplies a vector by a scalar.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector2 MulVectorScalar(PTVector2 a, double scalar)
		=> FromGDClass(a.vector * (float)scalar);

	/// <summary>
	/// Multiplies a scalar by a vector.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector2 MulScalarVector(double scalar, PTVector2 b)
		=> FromGDClass(b.vector * (float)scalar);

	/// <summary>
	/// Divides a vector by a scalar.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Div)]
	public static PTVector2 Div(PTVector2 a, double b)
	{
		return FromGDClass(a.vector / (float)b);
	}

	/// <summary>
	/// Returns the component-wise remainder of dividing one vector by another.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Mod)]
	public static PTVector2 Mod(PTVector2 a, PTVector2 b)
	{
		return FromGDClass(new Vector2(
			a.vector.X % b.vector.X,
			a.vector.Y % b.vector.Y
		));
	}

	/// <summary>
	/// Negates the vector.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Unm)]
	public static PTVector2 Unm(PTVector2 a)
	{
		return FromGDClass(-a.vector);
	}

	/// <summary>
	/// Raises each component of the vector to the power of the corresponding component of another vector.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Pow)]
	public static PTVector2 Pow(PTVector2 a, PTVector2 b)
	{
		return FromGDClass(new Vector2(
			(float)Math.Pow(a.vector.X, b.vector.X),
			(float)Math.Pow(a.vector.Y, b.vector.Y)
		));
	}

	/// <summary>
	/// Returns whether two vectors are equal.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Eq)]
	public static bool Eq(PTVector2 a, PTVector2 b)
	{
		return a.vector == b.vector;
	}

	/// <summary>
	/// Returns whether <paramref name="a"/> is less than <paramref name="b"/> by comparing their lengths.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Lt)]
	public static bool Lt(PTVector2 a, PTVector2 b)
	{
		return a.vector.LengthSquared() < b.vector.LengthSquared();
	}

	/// <summary>
	/// Returns whether <paramref name="a"/> is less than or equal to <paramref name="b"/> by comparing their lengths.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Le)]
	public static bool Le(PTVector2 a, PTVector2 b)
	{
		return a.vector.LengthSquared() <= b.vector.LengthSquared();
	}

	/// <summary>
	/// Returns the length (magnitude) of the vector.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.Len)]
	public static double Len(PTVector2 a)
	{
		return a.vector.Length();
	}

	/// <summary>
	/// Returns the string representation of the vector.
	/// </summary>
	[ScriptMetamethod(ScriptObjectMetamethod.ToString)]
	public static string ToString(PTVector2? v)
	{
		if (v == null) return "<Vector2>";
		return $"<Vector2:({v.vector.X}, {v.vector.Y})>";
	}

	/// <summary>
	/// Returns the angle in degrees between from and to.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float Angle(PTVector2 from, PTVector2 to) => from.vector.AngleTo(to.vector);

	/// <summary>
	/// Returns the cross product of lhs and rhs.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float Cross(PTVector2 lhs, PTVector2 rhs) => lhs.vector.Cross(rhs.vector);

	/// <summary>
	/// Returns the distance between a and b.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float Distance(PTVector2 a, PTVector2 b) => a.vector.DistanceTo(b.vector);

	/// <summary>
	/// Returns the dot product of lhs and rhs.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float Dot(PTVector2 lhs, PTVector2 rhs) => lhs.vector.Dot(rhs.vector);

	/// <summary>
	/// Returns a new vector that is the linear interpolation between a and b by t.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Lerp(PTVector2 a, PTVector2 b, float t) => FromGDClass(a.vector.Lerp(b.vector, t));

	/// <summary>
	/// Returns a vector that is made from the largest components of two vectors.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Max(PTVector2 lhs, PTVector2 rhs) => FromGDClass(lhs.vector.Max(rhs.vector));

	/// <summary>
	/// Returns a vector that is made from the smallest components of two vectors.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Min(PTVector2 lhs, PTVector2 rhs) => FromGDClass(lhs.vector.Min(rhs.vector));

	/// <summary>
	/// Calculate a position between the points specified by current and target, moving no farther than the distance specified by maxDistanceDelta.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 MoveTowards(PTVector2 current, PTVector2 target, float maxDistanceDelta) => FromGDClass(current.vector.MoveToward(target.vector, maxDistanceDelta));

	/// <summary>
	/// Returns a new Vector2 that is the normalized version of the given vector.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Normalize(PTVector2 value) => FromGDClass(value.vector.Normalized());

	/// <summary>
	/// Returns the projection of a vector onto another vector.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Project(PTVector2 vector, PTVector2 onNormal) => FromGDClass(vector.vector.Project(onNormal.vector));

	/// <summary>
	/// Returns the reflection of a vector off the plane defined by a normal.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Reflect(PTVector2 inDirection, PTVector2 inNormal) => FromGDClass(inDirection.vector.Reflect(inNormal.vector));

	/// <summary>
	/// Spherically interpolates between two vectors.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Slerp(PTVector2 a, PTVector2 b, float t) => FromGDClass(a.vector.Slerp(b.vector, t));

	/// <summary>
	/// Returns a new vector with each component rounded down to the nearest integer.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Floor(PTVector2 val) => FromGDClass(val.vector.Floor());

	/// <summary>
	/// Returns a new vector with each component rounded up to the nearest integer.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Ceil(PTVector2 val) => FromGDClass(val.vector.Ceil());

	/// <summary>
	/// Returns a new vector with each component rounded to the nearest integer.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Round(PTVector2 val) => FromGDClass(val.vector.Round());

	/// <summary>
	/// Returns a new vector with the absolute value of each component.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Abs(PTVector2 val) => FromGDClass(val.vector.Abs());

	/// <summary>
	/// Returns a new vector with the sign (-1, 0, or 1) of each component.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Sign(PTVector2 val) => FromGDClass(val.vector.Sign());

	/// <summary>
	/// Returns a new vector with each component clamped between the matching components of min and max.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Clamp(PTVector2 val, PTVector2 min, PTVector2 max) => FromGDClass(val.vector.Clamp(min.vector, max.vector));

	/// <summary>
	/// Returns the projection of a vector onto the plane defined by the given normal.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 ProjectOnPlane(PTVector2 vector, PTVector2 planeNormal) => FromGDClass(vector.vector.Slide(planeNormal.vector.Normalized()));

	/// <summary>
	/// Returns a copy of the vector rotated by the given angle in radians.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 Rotated(PTVector2 val, float angle) => FromGDClass(val.vector.Rotated(angle));

	/// <summary>
	/// Returns a copy of the vector with its length clamped to the given maximum length.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector2 LimitLength(PTVector2 val, float length) => FromGDClass(val.vector.LimitLength(length));

	/// <summary>
	/// Converts each component of the vector from radians to degrees.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTVector2 RadToDeg(PTVector2 val) => FromGDClass(new()
	{
		X = Mathf.RadToDeg(val.X),
		Y = Mathf.RadToDeg(val.Y),
	});

	/// <summary>
	/// Converts each component of the vector from degrees to radians.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTVector2 DegToRad(PTVector2 val) => FromGDClass(new()
	{
		X = Mathf.DegToRad(val.X),
		Y = Mathf.DegToRad(val.Y),
	});
}
