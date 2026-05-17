// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Scripting;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationTree is an anti-corruption layer for interfacing with Godot.AnimationTree.

/// <summary>
/// A node used for advanced animation transitions in an <see cref="AnimationPlayer"/>.
/// <para><strong>Note:</strong> When linked with an <see cref="AnimationPlayer"/>, several properties and methods of the corresponding <see cref="AnimationPlayer"/> will not function as expected. Playback and transitions should be handled using only the <c>AnimationTree</c> and its constituent <see cref="AnimationNode"/>(s). The <see cref="AnimationPlayer"/> node should be used solely for adding, deleting, and editing animations.</para>
/// </summary>
[Instantiable]
public partial class AnimationTree : AnimationMixer
{
    private Godot.AnimationTree _gdAnimTree = null!;

    private NodePath _advanceExpressionBaseNode = new(".");
    private NodePath _animPlayer = new("");
    private AnimationRootNode? _treeRoot = null;

    /// <summary>
    /// Emitted when the <see cref="AnimPlayer"/> is changed.
    /// </summary>
    [ScriptProperty]
    public PTSignal AnimationPlayerChanged { get; private set; } = new();

    /// <summary>
    /// The path to the <see cref="Node"/> used to evaluate the <see cref="AnimationNode"/> <see cref="Expression"/>
    /// if one is not explicitly specified internally.
    /// </summary>
    [Editable, ScriptProperty]
    public NodePath AdvanceExpressionBaseNode {
        get => _advanceExpressionBaseNode;
        set {
            _advanceExpressionBaseNode = value;
            _gdAnimTree.AdvanceExpressionBaseNode = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The path to the <see cref="AnimationPlayer"/> used for animating.
    /// <para><strong>Note:</strong> When linked with an <see cref="AnimationPlayer"/>, several properties and methods
    /// of the corresponding <see cref="AnimationPlayer"/> will not function as expected. Playback and transitions
    /// should be handled using only the <c>AnimationTree</c> and its constituent <see cref="AnimationNode"/>(s).
    /// The <see cref="AnimationPlayer"/> node should be used solely for adding, deleting, and editing animations.</para>
    /// </summary>
    [Editable, ScriptProperty]
    public NodePath AnimPlayer {
        get => _animPlayer;
        set {
            _animPlayer = value;
            _gdAnimTree.AnimPlayer = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The root animation node of this <c>AnimationTree</c>. See <see cref="AnimationRootNode"/>.
    /// </summary>
    [Editable, ScriptProperty]
    public AnimationRootNode? TreeRoot {
        get => _treeRoot;
        set {
            _treeRoot = value;
            _gdAnimTree.TreeRoot = value;
            OnPropertyChanged();
        }
    }

    public override Node CreateGDNode()
    {
        if (GDNode != null) return GDNode;
        _gdAnimTree = new Godot.AnimationTree();
        return _gdAnimTree;
    }

    public override void InitGDNode()
    {
        _gdAnimTree = (Godot.AnimationTree)GDNode;
        GDAnimMixer = _gdAnimTree;
        base.InitGDNode();
    }

    public override void Init()
    {
        _gdAnimTree.AnimationPlayerChanged += () => AnimationPlayerChanged.Invoke();
        base.Init();
    }
}