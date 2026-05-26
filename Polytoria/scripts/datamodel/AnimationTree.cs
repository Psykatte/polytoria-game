// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Scripting;
using Polytoria.Shared;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationTree is an anti-corruption layer for interfacing with Godot.AnimationTree.

/// <summary>
/// A node used for advanced animation transitions in an <see cref="AnimationPlayer"/>.
/// <para><strong>Note:</strong> When linked with an <see cref="AnimationPlayer"/>, several properties and methods of the corresponding <see cref="AnimationPlayer"/> will not function as expected. Playback and transitions should be handled using only the <c>AnimationTree</c> and its constituent <see cref="AnimationNode"/>(s). The <see cref="AnimationPlayer"/> node should be used solely for adding, deleting, and editing animations.</para>
/// </summary>
[Instantiable]
public partial class AnimationTree : AnimationMixer
{
    private Godot.AnimationTree GDAnimationTree = null!;
    protected override Godot.AnimationMixer GDAnimationMixer => GDAnimationTree;

    // private NodePath _advanceExpressionBaseNode = new(".");
    private AnimationPlayer? _animPlayer = null;
    // private AnimationRootNode? _treeRoot = null;

    /// <summary>
    /// The path to the <see cref="Node"/> used to evaluate the <see cref="AnimationNode"/> <see cref="Expression"/>
    /// if one is not explicitly specified internally.
    /// </summary>
/*     [Editable, ScriptProperty]
    public NodePath AdvanceExpressionBaseNode {
        get => _advanceExpressionBaseNode;
        set {
            _advanceExpressionBaseNode = value;
            GDAnimationTree.AdvanceExpressionBaseNode = value;
            OnPropertyChanged();
        }
    } */

    /// <summary>
    /// The path to the <see cref="AnimationPlayer"/> used for animating.
    /// <para><strong>Note:</strong> When linked with an <see cref="AnimationPlayer"/>, several properties and methods
    /// of the corresponding <see cref="AnimationPlayer"/> will not function as expected. Playback and transitions
    /// should be handled using only the <c>AnimationTree</c> and its constituent <see cref="AnimationNode"/>(s).
    /// The <see cref="AnimationPlayer"/> node should be used solely for adding, deleting, and editing animations.</para>
    /// </summary>
    [Editable, ScriptProperty]
    public AnimationPlayer? AnimPlayer {
        get => _animPlayer;
        set {
            _animPlayer = value;
            if (value != null)
            {
                value.GDAnimationPlayer.Reparent(GDAnimationTree); // TODO: Will break node paths!
                GDAnimationTree.AnimPlayer = value.GDAnimationPlayer.GetPath();
            }
            else GDAnimationTree.AnimPlayer = "";
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The root animation node of this <c>AnimationTree</c>. See <see cref="AnimationRootNode"/>.
    /// </summary>
/*     [Editable, ScriptProperty]
    public AnimationRootNode? TreeRoot {
        get => _treeRoot;
        set {
            _treeRoot = value;
            GDAnimationTree.TreeRoot = value;
            OnPropertyChanged();
        }
    } */

    /// <summary>
    /// Emitted when the <see cref="AnimPlayer"/> is changed.
    /// </summary>
    [ScriptProperty]
    public PTSignal AnimationPlayerChanged { get; private set; } = new();

	private void OnAnimationPlayerChanged()
    {
        AnimationPlayerChanged.Invoke();
    }
    
    public override Node CreateGDNode() => Globals.LoadNetworkedObjectScene(ClassName)!;

    public override void Init()
    {
        GDAnimationTree = (Godot.AnimationTree)GDNode;
        GDAnimationTree.AnimationPlayerChanged += OnAnimationPlayerChanged;
        base.Init();
    }

	public override void PreDelete()
	{
        GDAnimationTree.AnimationPlayerChanged -= OnAnimationPlayerChanged;
		base.PreDelete();
	}
}