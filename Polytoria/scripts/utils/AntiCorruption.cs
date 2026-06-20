// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using Godot;

namespace Polytoria.Utils;

// Godot's API guards bad input with ERR_FAIL_* macros: it prints an error and returns a default value rather than
// raising a catchable error, and several of those paths leave data inconsistent. In worse cases bad inputs can crash
// the engine or even expose vulnerabilities. These helpers are for validating every script-supplied argument up front
// and throwing a descriptive exception (the scripting bridge converts the message into a Luau error caught by pcall)
// before the Godot object is ever touched.
//
// Class-specific anti-corruption verifications should be added directly to the class.

public static class AntiCorruption
{
	// ------------------------------------------------- Gating --------------------------------------------------------

	// ScriptProperty setters validate their inputs and throw on invalid values. The same build runs the Creator, where
	// properties are edited live exceptions must not be thrown. The Creator and Client share the CREATOR binary, so
	// they can only be told apart at runtime: validation is skipped unless running as the client. Because places are
	// loaded through the property setters (see XmlFormat), this guarantees authored values are validated when the
	// client starts. In non-Creator builds validation is always on.
	public static bool ShouldValidate =>
#if CREATOR
		Polytoria.Shared.Globals.CurrentAppEntry == Polytoria.Shared.Globals.AppEntryEnum.Client;
#else
		true;
#endif

	// ------------------------------------------------- Values --------------------------------------------------------

	// Reject NaN/Infinity.
	public static void ValidateFinite<T>(
		T value,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	)
	{
		bool isFinite = value switch
		{
			float v => float.IsFinite(v),
			Vector2 v => v.IsFinite(),
			Vector3 v => v.IsFinite(),
			Quaternion v => v.IsFinite(),
			_ => throw new NotSupportedException($"Finite validation is not supported for type {typeof(T)}.")
		};

		if (!isFinite)
			throw new ArgumentException($"{paramName} must have finite components (got {value}).", paramName);
	}

	// Reject negative int.
	public static void ValidateNonNegative(
		int value,
		[CallerArgumentExpression(nameof(value))] string? paramName = null)
	{
		if (value < 0)
			throw new ArgumentException($"{paramName} must be non-negative (got {value}).", paramName);
	}

	// -------------------------------------------------- Names --------------------------------------------------------

	// Reject nil/null strings.
	public static void ValidateNameNotNil(
		string name,
		[CallerArgumentExpression(nameof(name))] string? paramName = null)
	{
		if (name is null)
			throw new ArgumentNullException(paramName, $"{paramName} cannot be nil.");
	}

	// Reject empty strings.
	public static void ValidateNameNotEmpty(
		string name,
		[CallerArgumentExpression(nameof(name))] string? paramName = null)
	{
		if (name is { Length: 0 })
			throw new ArgumentException($"{paramName} cannot be empty.", paramName);
	}

	// Reject nil/null/empty strings.
	public static void ValidateName(
		string name,
		[CallerArgumentExpression(nameof(name))] string? paramName = null)
	{
		ValidateNameNotNil(name, paramName);
		ValidateNameNotEmpty(name, paramName);
	}

	// -------------------------------------------------- Enums --------------------------------------------------------

	// Reject enum values outside the defined set.
	public static void ValidateEnum<T>(
		T value,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	) where T : struct, Enum
	{
		if (!Enum.IsDefined(value))
			throw new ArgumentException($"'{value}' is not a valid {typeof(T).Name} value.", paramName);
	}
}
