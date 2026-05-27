// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Utils;
using System;

namespace Polytoria.Scripting.Datatypes;

/// <summary>
/// Vector3 is a 3D vector with an x, y and z component.
/// </summary>
[DocCategory("types")]
public class PTVector3 : IScriptGDObject
{
	internal Vector3 vector;

	/// <summary>
	/// The X component of the vector.
	/// </summary>
	[ScriptProperty] public float X { get => vector.X; set => vector.X = value; }
	/// <summary>
	/// The Y component of the vector.
	/// </summary>
	[ScriptProperty] public float Y { get => vector.Y; set => vector.Y = value; }
	/// <summary>
	/// The Z component of the vector.
	/// </summary>
	[ScriptProperty] public float Z { get => vector.Z; set => vector.Z = value; }

	/// <summary>
	/// Shorthand for Vector3.New(0, 0, -1).
	/// </summary>
	[ScriptProperty] public static PTVector3 Forward { get; private set; } = new() { X = 0, Y = 0, Z = -1 };
	/// <summary>
	/// Shorthand for Vector3.New(0, 0, 1).
	/// </summary>
	[ScriptProperty] public static PTVector3 Back { get; private set; } = new() { X = 0, Y = 0, Z = 1 };
	/// <summary>
	/// Shorthand for Vector3.New(0, -1, 0).
	/// </summary>
	[ScriptProperty] public static PTVector3 Down { get; private set; } = new() { X = 0, Y = -1, Z = 0 };
	/// <summary>
	/// Shorthand for Vector3.New(-1, 0, 0).
	/// </summary>
	[ScriptProperty] public static PTVector3 Left { get; private set; } = new() { X = -1, Y = 0, Z = 0 };
	/// <summary>
	/// Shorthand for Vector3.New(1, 1, 1).
	/// </summary>
	[ScriptProperty] public static PTVector3 One { get; private set; } = new() { X = 1, Y = 1, Z = 1 };
	/// <summary>
	/// Shorthand for Vector3.New(0, 0, 0).
	/// </summary>
	[ScriptProperty] public static PTVector3 Zero { get; private set; } = new() { X = 0, Y = 0, Z = 0 };
	/// <summary>
	/// Shorthand for Vector3.New(1, 0, 0).
	/// </summary>
	[ScriptProperty] public static PTVector3 Right { get; private set; } = new() { X = 1, Y = 0, Z = 0 };
	/// <summary>
	/// Shorthand for Vector3.New(0, 1, 0).
	/// </summary>
	[ScriptProperty] public static PTVector3 Up { get; private set; } = new() { X = 0, Y = 1, Z = 0 };

	/// <summary>
	/// The length of the vector.
	/// </summary>
	[ScriptProperty] public float Magnitude => vector.Length();
	/// <summary>
	/// The normalized version of the vector.
	/// </summary>
	[ScriptProperty] public PTVector3 Normalized => FromGDClass(vector.Normalized());
	/// <summary>
	/// The squared length of the vector.
	/// </summary>
	[ScriptProperty] public float SqrMagnitude => vector.LengthSquared();

	public static PTVector3 FromGDClass(Vector3 vec)
	{
		return new PTVector3()
		{
			vector = vec
		};
	}

	public object ToGDClass()
	{
		return vector;
	}

	/// <summary>
	/// Returns a new Vector3 with the given Vector2 components and a z component of 0.
	/// </summary>
	/// <summary>
	/// Returns a new Vector3 with the given Vector2 components and a z component of 0.
	/// </summary>
	/// <summary>
	/// Returns a new Vector3 with the given Vector2 components and a z component of 0.
	/// </summary>
	/// <summary>
	/// Returns a new Vector3 with the given Vector2 components and a z component of 0.
	/// </summary>
	/// <summary>
	/// Returns a new Vector3 with the given Vector2 components and a z component of 0.
	/// </summary>
	[ScriptMethod]
	public static PTVector3 New()
	{
		return new()
		{
			X = 0,
			Y = 0,
			Z = 0
		};
	}

	[ScriptMethod]
	public static PTVector3 New(float d)
	{
		return new()
		{
			X = d,
			Y = d,
			Z = d
		};
	}

	[ScriptMethod]
	public static PTVector3 New(float x, float y)
	{
		return new()
		{
			X = x,
			Y = y,
			Z = 0
		};
	}

	[ScriptMethod]
	public static PTVector3 New(float x, float y, float z)
	{
		//PT.Print("New vector3: ", x, y, z);
		return new()
		{
			X = x,
			Y = y,
			Z = z
		};
	}

	[ScriptMethod]
	public static PTVector3 New(PTVector2 v)
	{
		return new()
		{
			X = v.X,
			Y = v.Y,
			Z = 0
		};
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Add)]
	public static PTVector3 Add(PTVector3 a, PTVector3 b)
	{
		return FromGDClass(a.vector + b.vector);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Sub)]
	public static PTVector3 SubVectorVector(PTVector3 a, PTVector3 b)
	=> FromGDClass(a.vector - b.vector);

	[ScriptMetamethod(ScriptObjectMetamethod.Sub)]
	public static PTVector3 SubVectorQuaternion(PTVector3 a, PTQuaternion q)
		=> FromGDClass(a.vector - new Vector3(q.X, q.Y, q.Z));

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector3 MulVectorVector(PTVector3 a, PTVector3 b)
	=> FromGDClass(a.vector * b.vector);

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector3 MulVectorScalar(PTVector3 a, double scalar)
		=> FromGDClass(a.vector * (float)scalar);

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector3 MulScalarVector(double scalar, PTVector3 b)
		=> FromGDClass(b.vector * (float)scalar);

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector3 MulVectorQuaternion(PTVector3 a, PTQuaternion q)
		=> FromGDClass(a.vector * q.quat.Normalized());

	[ScriptMetamethod(ScriptObjectMetamethod.Div)]
	public static PTVector3 Div(PTVector3 a, double b)
	{
		return FromGDClass(a.vector / (float)b);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Mod)]
	public static PTVector3 Mod(PTVector3 a, PTVector3 b)
	{
		return FromGDClass(new Vector3(
			a.vector.X % b.vector.X,
			a.vector.Y % b.vector.Y,
			a.vector.Z % b.vector.Z
		));
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Unm)]
	public static PTVector3 Unm(PTVector3 a)
	{
		return FromGDClass(-a.vector);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Pow)]
	public static PTVector3 Pow(PTVector3 a, PTVector3 b)
	{
		return FromGDClass(new Vector3(
			(float)Math.Pow(a.vector.X, b.vector.X),
			(float)Math.Pow(a.vector.Y, b.vector.Y),
			(float)Math.Pow(a.vector.Z, b.vector.Z)
		));
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Eq)]
	public static bool Eq(PTVector3 a, PTVector3 b)
	{
		return a.vector == b.vector;
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Lt)]
	public static bool Lt(PTVector3 a, PTVector3 b)
	{
		return a.vector.LengthSquared() < b.vector.LengthSquared();
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Le)]
	public static bool Le(PTVector3 a, PTVector3 b)
	{
		return a.vector.LengthSquared() <= b.vector.LengthSquared();
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Len)]
	public static double Len(PTVector3 a)
	{
		return a.vector.Length();
	}

	[ScriptMetamethod(ScriptObjectMetamethod.ToString)]
	public static string ToString(PTVector3? v)
	{
		if (v == null) return "<Vector3>";
		return $"<Vector3:({v.vector.X}, {v.vector.Y}, {v.vector.Z})>";
	}

	/// <summary>
	/// Returns the angle in degrees between from and to.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float Angle(PTVector3 from, PTVector3 to) => from.vector.AngleTo(to.vector);
	//[ScriptMethod] public static Vector3 ClampMagnitude(Vector3 vector, float maxLength) => vector.Clamp(vector, maxLength);
	/// <summary>
	/// Returns the cross product of lhs and rhs.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Cross(PTVector3 lhs, PTVector3 rhs) => FromGDClass(lhs.vector.Cross(rhs.vector));
	/// <summary>
	/// Returns the distance between a and b.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float Distance(PTVector3 a, PTVector3 b) => a.vector.DistanceTo(b.vector);
	/// <summary>
	/// Returns the dot product of lhs and rhs.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float Dot(PTVector3 lhs, PTVector3 rhs) => lhs.vector.Dot(rhs.vector);
	/// <summary>
	/// Returns a new Vector3 that is the linear interpolation between a and b by t.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Lerp(PTVector3 a, PTVector3 b, float t) => FromGDClass(a.vector.Lerp(b.vector, t));
	/// <summary>
	/// Returns a vector that is made from the largest components of two vectors.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Max(PTVector3 lhs, PTVector3 rhs) => FromGDClass(lhs.vector.Max(rhs.vector));
	/// <summary>
	/// Returns a vector that is made from the smallest components of two vectors.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Min(PTVector3 lhs, PTVector3 rhs) => FromGDClass(lhs.vector.Min(rhs.vector));
	/// <summary>
	/// Calculate a position between the points specified by current and target, moving no farther than the distance specified by maxDistanceDelta.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 MoveTowards(PTVector3 current, PTVector3 target, float maxDistanceDelta) => FromGDClass(current.vector.MoveToward(target.vector, maxDistanceDelta));
	/// <summary>
	/// Returns a new Vector3 that is the normalized version of the given vector.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Normalize(PTVector3 value) => FromGDClass(value.vector.Normalized());
	/// <summary>
	/// Returns the projection of a vector onto another vector.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Project(PTVector3 vector, PTVector3 onNormal) => FromGDClass(vector.vector.Project(onNormal.vector));
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 ProjectOnPlane(PTVector3 vector, PTVector3 planeNormal) => FromGDClass(vector.vector.Slide(planeNormal.vector.Normalized()));
	/// <summary>
	/// Returns the reflection of a vector off the plane defined by a normal.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Reflect(PTVector3 inDirection, PTVector3 inNormal) => FromGDClass(inDirection.vector.Reflect(inNormal.vector));
	//[ScriptMethod] public static Vector3 RotateTowards(Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta) => current.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
	//public static Vector3 Scale(Vector3 a, Vector3 b) => a.Scale(b);
	/// <summary>
	/// Returns the signed angle in degrees between from and to.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static float SignedAngle(PTVector3 from, PTVector3 to, PTVector3 axis) => from.vector.SignedAngleTo(to.vector, axis.vector);

	/// <summary>
	/// Spherically interpolates between two vectors.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTVector3 Slerp(PTVector3 a, PTVector3 b, float t)
	{
		Vector3 normalizedA = a.vector.Normalized();
		Vector3 normalizedB = b.vector.Normalized();
		return FromGDClass(normalizedA.Slerp(normalizedB, t));
	}
	//public static Vector3 SlerpUnclamped(Vector3 a, Vector3 b, float t) => a.SlerpUnclamped(b, t);
	//public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime) => current.SmoothDamp(target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Floor(PTVector3 val) => FromGDClass(val.vector.Floor());
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Ceil(PTVector3 val) => FromGDClass(val.vector.Ceil());
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Round(PTVector3 val) => FromGDClass(val.vector.Round());
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Abs(PTVector3 val) => FromGDClass(val.vector.Abs());
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Sign(PTVector3 val) => FromGDClass(val.vector.Sign());
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Rotated(PTVector3 val, PTVector3 axis, float angle) => FromGDClass(val.vector.Rotated(axis.vector, angle));
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 LimitLength(PTVector3 val, float length) => FromGDClass(val.vector.LimitLength(length));
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 Clamp(PTVector3 val, PTVector3 min, PTVector3 max) => FromGDClass(val.vector.Clamp(min.vector, max.vector));
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 RadToDeg(PTVector3 val) => FromGDClass(val.vector.RadToDeg());
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)] public static PTVector3 DegToRad(PTVector3 val) => FromGDClass(val.vector.DegToRad());
}
