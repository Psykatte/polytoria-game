// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Godot;
using Polytoria.Attributes;
using Polytoria.Enums;
using Polytoria.Scripting;

using static Polytoria.Utils.AntiCorruption;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationTree is an anti-corruption layer for interfacing with Godot.AnimationTree.

/// <summary>
/// A node used for advanced animation transitions in an <see cref="AnimationPlayer"/>.
/// <para><strong>Note:</strong> When linked with an <see cref="AnimationPlayer"/>, several properties and methods of the corresponding <see cref="AnimationPlayer"/> will not function as expected. Playback and transitions should be handled using only the <see cref="AnimationTree"/> and its constituent <see cref="AnimationNode"/>(s). The <see cref="AnimationPlayer"/> node should be used solely for adding, deleting, and editing <see cref="Animation"/>s.</para>
/// </summary>
[Instantiable]
public partial class AnimationTree : AnimationMixer
{
	// -----------------------------------------------------------------------------------------------------------------
	// Internal Data

	private Godot.AnimationTree GDAnimationTree = null!;
	protected override Godot.AnimationMixer GDAnimationMixer => GDAnimationTree;

	private string _advanceExpressionBaseNode = ".";
	private string _animPlayer = "";

	// -----------------------------------------------------------------------------------------------------------------
	// Exposed Properties

	/// <summary>
	/// The path to the <see cref="Node"/> used to evaluate the <see cref="AnimationNode"/> <c>Expression</c>
	/// if one is not explicitly specified internally.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(".")]
	public string AdvanceExpressionBaseNode
	{
		get => _advanceExpressionBaseNode;
		set
		{
			if (ShouldValidate && value is null)
				throw new ArgumentNullException(nameof(value), "AdvanceExpressionBaseNode cannot be nil.");

			_advanceExpressionBaseNode = value;
			GDAnimationTree.AdvanceExpressionBaseNode = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The path to the <see cref="AnimationPlayer"/> used for animating.
	/// <para><strong>Note:</strong> When linked with an <see cref="AnimationPlayer"/>, several properties and methods
	/// of the corresponding <see cref="AnimationPlayer"/> will not function as expected. Playback and transitions
	/// should be handled using only the <see cref="AnimationTree"/> and its constituent <see cref="AnimationNode"/>(s).
	/// The <see cref="AnimationPlayer"/> node should be used solely for adding, deleting, and editing <see cref="Animation"/>s.</para>
	/// </summary>
	[Editable, ScriptProperty, DefaultValue("")]
	public string AnimPlayer
	{
		get => _animPlayer;
		set
		{
			if (ShouldValidate && value is null)
				throw new ArgumentNullException(nameof(value), "AnimPlayer cannot be nil.");

			_animPlayer = value;
			GDAnimationTree.AnimPlayer = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Ordinarily, tracks can be set to <c>Animation.UPDATE_DISCRETE</c> to update infrequently, usually when using nearest interpolation.
	/// However, when blending with <c>Animation.UPDATE_CONTINUOUS</c> several results are considered. The <c>callback_mode_discrete</c> specify it explicitly.
	/// To make the blended results look good, it is recommended to set this to <c>ANIMATION_CALLBACK_MODE_DISCRETE_FORCE_CONTINUOUS</c> to update every frame during blending.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(AnimationCallbackModeDiscreteEnum.ForceContinuous)]
	public override AnimationCallbackModeDiscreteEnum CallbackModeDiscrete
	{
		get => _callbackModeDiscrete;
		set
		{
			if (ShouldValidate)
				ValidateEnum(value, nameof(value));

			_callbackModeDiscrete = value;
			GDAnimationMixer.CallbackModeDiscrete = (Godot.AnimationMixer.AnimationCallbackModeDiscrete)(int)value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// If <c>true</c>, the blending uses the deterministic algorithm. The total weight is not normalized and the result is accumulated with an initial value (<c>0</c> or a <c>"RESET"</c> animation if present).
	/// If <c>false</c>, The blend does not use the deterministic algorithm. The total weight is normalized and always <c>1.0</c>.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(true)]
	public override bool Deterministic
	{
		get => base.Deterministic;
		set => base.Deterministic = value;
	}

	// Not exposed: requires an AnimationRootNode/AnimationNode ACL which doesn't exist yet.
	/// <summary>
	/// The root animation node of this <c>AnimationTree</c>. See <see cref="AnimationRootNode"/>.
	/// </summary>
	/*     [Editable, ScriptProperty, DefaultValue(null)]
		public AnimationRootNode? TreeRoot {
			get => _treeRoot;
			set {
				_treeRoot = value;
				GDAnimationTree.TreeRoot = value;
				OnPropertyChanged();
			}
		} */

	// -----------------------------------------------------------------------------------------------------------------
	// Exposed Signals

	/// <summary>
	/// Emitted when the <see cref="AnimPlayer"/> is changed.
	/// </summary>
	[ScriptProperty]
	public PTSignal AnimationPlayerChanged { get; private set; } = new();

	private void OnAnimationPlayerChanged()
	{
		AnimationPlayerChanged.Invoke();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// Initialization and Cleanup

	public override Node CreateGDNode()
	{
		return new Godot.AnimationTree();
	}

	public override void InitGDNode()
	{
		GDAnimationTree = (Godot.AnimationTree)GDNode;
		base.InitGDNode();
	}

	public override void Init()
	{
		GDAnimationTree.AnimationPlayerChanged += OnAnimationPlayerChanged;
		base.Init();
	}

	public override void PreDelete()
	{
		GDAnimationTree.AnimationPlayerChanged -= OnAnimationPlayerChanged;
		base.PreDelete();
	}
}
