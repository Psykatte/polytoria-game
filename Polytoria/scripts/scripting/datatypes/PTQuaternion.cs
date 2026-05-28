// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Utils;

namespace Polytoria.Scripting.Datatypes;

// NOTE: Quaternion exposed to developers is in degrees
/// <summary>
/// Represents a quaternion used for rotations.
/// </summary>
[DocCategory("types")]
public class PTQuaternion : IScriptGDObject
{
	internal Quaternion quat;

	/// <summary>
	/// The X component of the quaternion.
	/// </summary>
	[ScriptProperty] public float X { get => quat.X; set => quat.X = value; }

	/// <summary>
	/// The Y component of the quaternion.
	/// </summary>
	[ScriptProperty] public float Y { get => quat.Y; set => quat.Y = value; }

	/// <summary>
	/// The Z component of the quaternion.
	/// </summary>
	[ScriptProperty] public float Z { get => quat.Z; set => quat.Z = value; }

	/// <summary>
	/// The W component of the quaternion.
	/// </summary>
	[ScriptProperty] public float W { get => quat.W; set => quat.W = value; }

	/// <summary>
	/// The identity rotation.
	/// </summary>
	[ScriptProperty] public static PTQuaternion Identity => new() { X = 0, Y = 0, Z = 0, W = 1 };

	public static PTQuaternion FromGDClass(Quaternion qu)
	{
		return new PTQuaternion()
		{
			quat = qu
		};
	}

	public object ToGDClass()
	{
		return quat;
	}

	/// <summary>
	/// Creates a new Quaternion object with the specified components.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion New()
	{
		return Identity;
	}

	/// <summary>
	/// Creates a new Quaternion object with the specified components.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion New(float x, float y, float z, float w)
	{
		return new()
		{
			X = x,
			Y = y,
			Z = z,
			W = w
		};
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Add)]
	public static PTQuaternion Add(PTQuaternion a, PTQuaternion b)
	{
		return FromGDClass(a.quat + b.quat);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Sub)]
	public static PTQuaternion SubQuaternionQuaternion(PTQuaternion a, PTQuaternion b)
		=> FromGDClass(a.quat - b.quat);

	[ScriptMetamethod(ScriptObjectMetamethod.Sub)]
	public static PTQuaternion SubQuaternionVector(PTQuaternion a, PTVector3 v)
		=> FromGDClass(a.quat - new Quaternion(v.X, v.Y, v.Z, 1));

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTQuaternion MulQuaternionQuaternion(PTQuaternion a, PTQuaternion b)
		=> FromGDClass(a.quat.Normalized() * b.quat.Normalized());

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector3 MulQuaternionVector(PTQuaternion a, PTVector3 v)
		=> PTVector3.FromGDClass(a.quat.Normalized() * v.vector);

	[ScriptMetamethod(ScriptObjectMetamethod.Mul)]
	public static PTVector3 MulVectorQuaternion(PTVector3 v, PTQuaternion q)
	=> PTVector3.FromGDClass(q.quat.Normalized() * v.vector);

	[ScriptMetamethod(ScriptObjectMetamethod.Eq)]
	public static bool Eq(PTQuaternion a, PTQuaternion b)
	{
		return a.quat == b.quat;
	}

	[ScriptMetamethod(ScriptObjectMetamethod.ToString)]
	public static string ToString(PTQuaternion? v)
	{
		if (v == null) return "<Quaternion>";
		return $"<Quaternion:({v.quat.X}, {v.quat.Y}, {v.quat.Z}, {v.quat.W}>";
	}

	/// <summary>
	/// Calculates the angle between two quaternions.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static float Angle(PTQuaternion a, PTQuaternion b)
	{
		// Angle still Works with Deg
		return Mathf.RadToDeg(a.quat.AngleTo(b.quat));
	}

	/// <summary>
	/// Creates a rotation which rotates angle degrees around axis.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion AngleAxis(float angle, Vector3 axis)
	{
		return FromGDClass(new Quaternion(axis, angle));
	}

	/// <summary>
	/// Calculates the dot product of two quaternions.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static float Dot(PTQuaternion a, PTQuaternion b)
	{
		return a.quat.Dot(b.quat);
	}

	/// <summary>
	/// Creates a quaternion from Euler angles specified by a Vector3.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion Euler(float x, float y, float z)
	{
		return FromGDClass(Quaternion.FromEuler(MathUtils.Vector3DegToRad(new(x, y, z))));
	}

	/// <summary>
	/// Creates a quaternion from Euler angles specified by a Vector3.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion Euler(Vector3 euler)
	{
		return FromGDClass(Quaternion.FromEuler(euler.DegToRad()));
	}


	/// <summary>
	/// Converts a quaternion to Euler angles represented as a Vector3.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static Vector3 ToEuler(PTQuaternion euler)
	{
		return MathUtils.Vector3RadToDeg(euler.quat.GetEuler());
	}

	/// <summary>
	/// Creates a rotation which rotates angle degrees around axis.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
	{
		Vector3 from = fromDirection.Normalized();
		Vector3 to = toDirection.Normalized();

		float dot = from.Dot(to);

		// same direction
		if (dot >= 1.0f - 1e-6f)
			return FromGDClass(Quaternion.Identity);

		// opposite directions
		if (dot <= -1.0f + 1e-6f)
		{
			Vector3 perpendicular = from.Cross(Vector3.Up);
			if (perpendicular.LengthSquared() < 1e-6f)
				perpendicular = from.Cross(Vector3.Right);
			return FromGDClass(new Quaternion(perpendicular.Normalized(), Mathf.Pi));
		}

		Vector3 axis = from.Cross(to).Normalized();
		float angle = Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f));
		return FromGDClass(new Quaternion(axis, angle));
	}

	/// <summary>
	/// Calculates the inverse of a quaternion.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTQuaternion Inverse(PTQuaternion rotation)
	{
		return FromGDClass(rotation.quat.Inverse());
	}

	/// <summary>
	/// Linearly interpolates between two quaternions.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTQuaternion Lerp(PTQuaternion a, PTQuaternion b, float t)
	{
		Quaternion q = new(
			Mathf.Lerp(a.quat.X, b.quat.X, t),
			Mathf.Lerp(a.quat.Y, b.quat.Y, t),
			Mathf.Lerp(a.quat.Z, b.quat.Z, t),
			Mathf.Lerp(a.quat.W, b.quat.W, t)
		);
		q = q.Normalized();
		return FromGDClass(q);
	}

	/// <summary>
	/// Linearly interpolates between two quaternions without clamping the interpolant.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTQuaternion LerpUnclamped(PTQuaternion a, PTQuaternion b, float t)
	{
		Quaternion q = new(
			Mathf.Lerp(a.quat.X, b.quat.X, t),
			Mathf.Lerp(a.quat.Y, b.quat.Y, t),
			Mathf.Lerp(a.quat.Z, b.quat.Z, t),
			Mathf.Lerp(a.quat.W, b.quat.W, t)
		);
		return FromGDClass(q);
	}

	/// <summary>
	/// Creates a rotation with the specified forward and upwards directions.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion LookRotation(Vector3 forward)
	{
		return LookRotation(forward, Vector3.Up);
	}

	/// <summary>
	/// Creates a rotation with the specified forward and upwards directions.
	/// </summary>
	[ScriptMethod]
	public static PTQuaternion LookRotation(Vector3 forward, Vector3 upwards)
	{
		forward = forward.Normalized();
		upwards = upwards.Normalized();

		var basis = Basis.LookingAt(-forward, upwards);
		return FromGDClass(basis.GetRotationQuaternion());
	}

	/// <summary>
	/// Normalizes the given quaternion.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTQuaternion Normalize(PTQuaternion quaternion)
	{
		return FromGDClass(quaternion.quat.Normalized());
	}

	/// <summary>
	/// Rotates a rotation from towards to by maxDegreesDelta.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTQuaternion RotateTowards(PTQuaternion from, PTQuaternion to, float maxDegreesDelta)
	{
		Quaternion fromQ = from.quat;
		Quaternion toQ = to.quat;

		float angle = fromQ.AngleTo(toQ);
		float maxRadiansDelta = Mathf.DegToRad(maxDegreesDelta);

		if (angle == 0)
			return to; // already same rotation

		// Determine interpolation factor
		float t = Mathf.Min(1f, maxRadiansDelta / angle);
		Quaternion result = fromQ.Slerp(toQ, t);

		return FromGDClass(result);
	}

	/// <summary>
	/// Spherically interpolates between two quaternions.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTQuaternion Slerp(PTQuaternion a, PTQuaternion b, float t)
	{
		return FromGDClass(a.quat.Slerp(b.quat, t));
	}

	/// <summary>
	/// Spherically interpolates between two quaternions without clamping the interpolant.
	/// </summary>
	[ScriptMethod(ConvertParamsToGD = false, SemiStatic = true)]
	public static PTQuaternion SlerpUnclamped(PTQuaternion a, PTQuaternion b, float t)
	{
		return FromGDClass(a.quat.Slerpni(b.quat, t));
	}

}
