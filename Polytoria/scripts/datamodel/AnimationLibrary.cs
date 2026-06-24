// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using Godot;
using Polytoria.Attributes;
using Polytoria.Enums;
using Polytoria.Scripting;

using static Polytoria.Utils.AntiCorruption;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationLibrary is an anti-corruption layer for interfacing with Godot.AnimationLibrary.

/// <summary>
/// An animation library stores a set of <see cref="Animation"/>s accessible through string keys, for use with <see cref="AnimationPlayer"/> nodes.
/// </summary>
[Instantiable]
public partial class AnimationLibrary : Instance
{
	// -----------------------------------------------------------------------------------------------------------------
	// Internal Data

	private static readonly ConditionalWeakTable<Godot.AnimationLibrary, AnimationLibrary> GDAnimationLibraries = [];
	private Godot.AnimationLibrary GDAnimationLibrary = null!;

	// -----------------------------------------------------------------------------------------------------------------
	// Exposed Scripting Methods

	/// <summary>
	/// Adds the <paramref name="animation"> to the library, accessible by the key <paramref name="name">.
	/// </summary>
	/// <param name="name">The name of the key used to access the stored <see cref="Animation">.</param>
	/// <param name="animation">The <see cref="Animation"> to store.</param>
	/// <returns>The <see cref="ErrorEnum"> returned by Godot.</returns>
	[ScriptMethod]
	public ErrorEnum AddAnimation(string name, Animation animation)
	{
		ValidateName(name);
		if (animation is null)
			throw new ArgumentNullException(nameof(animation), "Animation cannot be nil.");

		return (ErrorEnum)GDAnimationLibrary.AddAnimation(name, animation);
	}

	/// <summary>
	/// Returns the <see cref="Animation"> with the key <paramref name="name">.
	/// </summary>
	/// <param name="name">The key of the animation to retrieve.</param>
	/// <returns>The <see cref="Animation"> if found, otherwise <c>null</c>.</returns>
	[ScriptMethod]
	public Animation? GetAnimation(string name)
	{
		ValidateName(name);

		return GDAnimationLibrary.GetAnimation(name);
	}

	/// <summary>
	/// Returns the keys for the <see cref="Animation">s stored in the library.
	/// </summary>
	/// <returns>An array of string keys.</returns>
	[ScriptMethod]
	public string[] GetAnimationList()
	{
		var array = GDAnimationLibrary.GetAnimationList();
		var result = new string[array.Count];
		for (int i = 0; i < array.Count; i++)
			result[i] = (string)array[i];
		return result;
	}

	/// <summary>
	/// Returns the number of animations stored in the library.
	/// </summary>
	/// <returns>The count of animation keys.</returns>
	[ScriptMethod]
	public int GetAnimationListSize()
	{
		return GDAnimationLibrary.GetAnimationListSize();
	}

	/// <summary>
	/// Checks if the library stores an <see cref="Animation"> with the specified key.
	/// </summary>
	/// <param name="name">The key to check.</param>
	/// <returns><c>true</c> if the animation exists, otherwise <c>false</c>.</returns>
	[ScriptMethod]
	public bool HasAnimation(string name)
	{
		ValidateName(name);

		return GDAnimationLibrary.HasAnimation(name);
	}

	/// <summary>
	/// Removes the <see cref="Animation"> with the specified key.
	/// </summary>
	/// <param name="name">The key of the animation to remove.</param>
	[ScriptMethod]
	public void RemoveAnimation(string name)
	{
		ValidateAnimationExists(name);

		GDAnimationLibrary.RemoveAnimation(name);
	}

	/// <summary>
	/// Changes the key of an <see cref="Animation"> from <paramref name="name"> to <paramref name="newname">.
	/// </summary>
	/// <param name="name">The current key of the animation.</param>
	/// <param name="newname">The new key for the animation.</param>
	[ScriptMethod]
	public void RenameAnimation(string name, string newname)
	{
		ValidateAnimationExists(name);
		ValidateName(newname);
		if (GDAnimationLibrary.HasAnimation(newname))
			throw new ArgumentException($"An animation with the name '{newname}' already exists in this library.", nameof(newname));

		GDAnimationLibrary.RenameAnimation(name, newname);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// Exposed Signals

	/// <summary>
	/// Emitted when an <see cref="Animation"> is added to the library.
	/// </summary>
	[ScriptProperty]
	public PTSignal<string> AnimationAdded { get; private set; } = new();

	/// <summary>
	/// Emitted when an <see cref="Animation"> in the library is modified.
	/// </summary>
	[ScriptProperty]
	public PTSignal<string> AnimationChanged { get; private set; } = new();

	/// <summary>
	/// Emitted when an <see cref="Animation"> is removed from the library.
	/// </summary>
	[ScriptProperty]
	public PTSignal<string> AnimationRemoved { get; private set; } = new();

	/// <summary>
	/// Emitted when an <see cref="Animation"> key is renamed.
	/// </summary>
	[ScriptProperty]
	public PTSignal<string> AnimationRenamed { get; private set; } = new();

	private void OnAnimationAdded(StringName name)
	{
		AnimationAdded.Invoke((string)name);
	}

	private void OnAnimationChanged(StringName name)
	{
		AnimationChanged.Invoke((string)name);
	}

	private void OnAnimationRemoved(StringName name)
	{
		AnimationRemoved.Invoke((string)name);
	}

	private void OnAnimationRenamed(StringName name, StringName toName)
	{
		AnimationRenamed.Invoke((string)name, (string)toName);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// Initialization and Cleanup

	public override void Init()
	{
		GDAnimationLibrary ??= new Godot.AnimationLibrary();
		GDAnimationLibraries.Add(GDAnimationLibrary, this);
		GDAnimationLibrary.AnimationAdded += OnAnimationAdded;
		GDAnimationLibrary.AnimationChanged += OnAnimationChanged;
		GDAnimationLibrary.AnimationRemoved += OnAnimationRemoved;
		GDAnimationLibrary.AnimationRenamed += OnAnimationRenamed;
		base.Init();
	}

	public override void PreDelete()
	{
		GDAnimationLibrary.AnimationAdded -= OnAnimationAdded;
		GDAnimationLibrary.AnimationChanged -= OnAnimationChanged;
		GDAnimationLibrary.AnimationRemoved -= OnAnimationRemoved;
		GDAnimationLibrary.AnimationRenamed -= OnAnimationRenamed;
		GDAnimationLibrary.Dispose();
		base.PreDelete();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// Internal Conversions

	// Intialize an AnimationLibrary from a Godot type, this is done to mitigate possible memory leaks.
	private static AnimationLibrary FromGDObject(Godot.AnimationLibrary gdAnimationLibrary)
	{
		return Polytoria.Shared.Globals.LoadInstance<AnimationLibrary>(World.Current, lib =>
			lib.GDAnimationLibrary = gdAnimationLibrary);
	}

	// Implicit conversion from ACL type to Godot type.
	public static implicit operator Godot.AnimationLibrary(AnimationLibrary acl) => acl.GDAnimationLibrary;

	// Implicit conversion from Godot type to ACL type.
	public static implicit operator AnimationLibrary?(Godot.AnimationLibrary? gd) =>
		gd is null ? null : GDAnimationLibraries.GetOrAdd(gd, _ => FromGDObject(gd));

	// -----------------------------------------------------------------------------------------------------------------
	// Private Validation

	// Ensures an animation with the given key exists before reading, renaming, or removing it.
	private void ValidateAnimationExists(string name)
	{
		ValidateName(name);
		if (!GDAnimationLibrary.HasAnimation(name))
			throw new ArgumentException($"No animation with the key '{name}' exists in this library.", nameof(name));
	}
}
