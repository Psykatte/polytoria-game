// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using System;

namespace Polytoria.Scripting.Datatypes;

/// <summary>
/// StringNames are immutable strings designed for general-purpose representation
/// of unique names. StringName ensures that only one instance of a given name exists
/// (so two StringNames with the same value are the same object). Comparing them
/// is much faster than with regular strings, because only the pointers are compared,
/// not the whole strings.
/// </summary>
public class PTStringName : IScriptGDObject
{
	internal StringName stringName = null!;

	/// <summary>
	/// Constructs an empty <strong>StringName</strong>.
	/// </summary>
	[ScriptMethod]
	public static PTStringName New()
	{
		return new()
		{
			stringName = new StringName()
		};
	}


	/// <summary>
	/// Constructs a <strong>StringName</strong> as a copy of the given <strong>StringName</strong>.
	/// </summary>
	[ScriptMethod]
	public static PTStringName New(StringName str)
	{
		return new()
		{
			stringName = new StringName(str)
		};
	}

	/// <summary>
	/// Creates a new <strong>StringName</strong> from the given <c>String</c>.
	/// </summary>
	[ScriptMethod]
	public static PTStringName New(string str)
	{
		return new()
		{
			stringName = (StringName)str
		};
	}

	public static PTStringName FromGDClass(String str)
	{
		return new PTStringName()
		{
			stringName = (StringName)str
		};
	}

	public object ToGDClass()
	{
		return stringName;
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Eq)]
	public static bool Eq(PTStringName a, PTStringName b)
	{
		return (a.stringName == b.stringName);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Eq)]
	public static bool Eq(String a, PTStringName b)
	{
		return (a == b.stringName);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Eq)]
	public static bool Eq(PTStringName a, String b)
	{
		return (a.stringName == b);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Neq)]
	public static bool Neq(PTStringName a, PTStringName b)
	{
		return (a.stringName != b.stringName);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Neq)]
	public static bool Neq(String a, PTStringName b)
	{
		return (a != b.stringName);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.Neq)]
	public static bool Neq(PTStringName a, String b)
	{
		return (a.stringName != b);
	}

	[ScriptMetamethod(ScriptObjectMetamethod.ToString)]
	public static string ToString(PTStringName? str)
	{
		if (str == null) return "<StringName>";
		return $"<StringName:{str.stringName}>";
	}
}
