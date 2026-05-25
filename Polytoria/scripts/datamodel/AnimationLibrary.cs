// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime.CompilerServices;
using Godot;
using Polytoria.Attributes;
using Polytoria.Enums;
using Polytoria.Scripting;
using Polytoria.Scripting.Datatypes;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationMixer is an anti-corruption layer for interfacing with Godot.AnimationMixer.

/// <summary>
/// An animation library stores a set of animations accessible through <see cref="StringName"> keys, for use with <see cref="AnimationPlayer"> nodes.
/// </summary>
[Instantiable]
public partial class AnimationLibrary : Instance
{
    private static readonly ConditionalWeakTable<Godot.AnimationLibrary, AnimationLibrary> GDAnimationLibraries = [];
    private Node GDHostNode = null!;
    private Godot.AnimationLibrary GDAnimationLibrary = null!;

    // Intialize an AnimationLibrary from a Godot type, this is done to mitigate possible memory leaks.
    private static AnimationLibrary FromGDObject(Godot.AnimationLibrary gdAnimationLibrary)
    {
        return Polytoria.Shared.Globals.LoadInstance<AnimationLibrary>(World.Current, lib => lib.GDAnimationLibrary = gdAnimationLibrary);
    }

    // Implicit conversion from ACL type to Godot type.
    public static implicit operator Godot.AnimationLibrary?(AnimationLibrary acl) => acl?.GDAnimationLibrary;

	// Implicit conversion from Godot type to ACL type.
    public static implicit operator AnimationLibrary(Godot.AnimationLibrary gd) => GDAnimationLibraries.GetOrAdd(gd, _ => FromGDObject(gd));

    /// <summary>
    /// Emitted when an <see cref="Animation"> is added to the library.
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName> AnimationAdded { get; private set; } = new();

    /// <summary>
    /// Emitted when an <see cref="Animation"> in the library is modified.
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName> AnimationChanged { get; private set; } = new();

    /// <summary>
    /// Emitted when an <see cref="Animation"> is removed from the library.
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName> AnimationRemoved { get; private set; } = new();

    /// <summary>
    /// Emitted when an <see cref="Animation"> key is renamed.
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName> AnimationRenamed{ get; private set; } = new();

    private void OnAnimationAdded(StringName name)
    {
        AnimationAdded.Invoke(name);
    }

    private void OnAnimationChanged(StringName name)
    {
        AnimationChanged.Invoke(name);
    }

    private void OnAnimationRemoved(StringName name)
    {
        AnimationRemoved.Invoke(name);
    }

    private void OnAnimationRenamed(StringName name, StringName toName)
    {
        AnimationRenamed.Invoke(name, toName);
    }

    public override void Init()
    {
        GDHostNode = (Godot.Node)GDNode;
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

    /// <summary>
    /// Adds the <paramref name="animation"> to the library, accessible by the key <paramref name="name">.
    /// </summary>
    /// <param name="name">The name of the key used to access the stored <see cref="Animation">.</param>
    /// <param name="animation">The <see cref="Animation"> to store.</param>
    /// <returns>The <see cref="ErrorEnum"> returned by Godot.</returns>
    [ScriptMethod]
    public ErrorEnum AddAnimation(PTStringName name, Animation animation)
    {
        return (ErrorEnum)GDAnimationLibrary.AddAnimation(name, animation);
    }

    /// <summary>
    /// Returns the <see cref="Animation"> with the key <paramref name="name">.
    /// </summary>
    /// <param name="name">The key of the animation to retrieve.</param>
    /// <returns>The <see cref="Animation"> if found, otherwise <c>null</c>.</returns>
    [ScriptMethod]
    public Animation? GetAnimation(PTStringName name)
    {
        return GDAnimationLibrary.GetAnimation(name);
    }

    /// <summary>
    /// Returns the keys for the <see cref="Animation">s stored in the library.
    /// </summary>
    /// <returns>An array of <see cref="StringName"> keys.</returns>
    [ScriptMethod]
    public PTStringName[] GetAnimationList()
    {
        var array = GDAnimationLibrary.GetAnimationList();
        var result = new PTStringName[array.Count];
        for (int i = 0; i < array.Count; i++)
        {
            result[i] = (PTStringName)array[i];
        }
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
    public bool HasAnimation(PTStringName name)
    {
        return GDAnimationLibrary.HasAnimation(name);
    }

    /// <summary>
    /// Removes the <see cref="Animation"> with the specified key.
    /// </summary>
    /// <param name="name">The key of the animation to remove.</param>
    [ScriptMethod]
    public void RemoveAnimation(PTStringName name)
    {
        GDAnimationLibrary.RemoveAnimation(name);
    }

    /// <summary>
    /// Changes the key of an <see cref="Animation"> from <paramref name="name"> to <paramref name="newname">.
    /// </summary>
    /// <param name="name">The current key of the animation.</param>
    /// <param name="newname">The new key for the animation.</param>
    [ScriptMethod]
    public void RenameAnimation(PTStringName name, PTStringName newname)
    {
        GDAnimationLibrary.RenameAnimation(name, newname);
    }
}