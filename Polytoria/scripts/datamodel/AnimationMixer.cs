// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Enums;
using Polytoria.Scripting;
using Polytoria.Scripting.Datatypes;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationMixer is an anti-corruption layer for interfacing with Godot.AnimationMixer.

/// <summary>
/// <para>Base class for <see cref="AnimationPlayer"/> and <see cref="AnimationTree"/> to manage animation lists. It also has general properties and methods for playback and blending.</para>
/// <para>After instantiating the playback information data within the extended class, the blending is processed by the <c>AnimationMixer</c>.</para>
/// </summary>
[Abstract]
public abstract partial class AnimationMixer : Instance
{
    protected abstract Godot.AnimationMixer GDAnimationMixer { get; }
    private bool _active = true;
    private int _audioMaxPolyphony = 32;
    private AnimationCallbackModeDiscreteEnum _callbackModeDiscrete = AnimationCallbackModeDiscreteEnum.Recessive;
    private AnimationCallbackModeMethodEnum _callbackModeMethod = AnimationCallbackModeMethodEnum.Deferred;
    private AnimationCallbackModeProcessEnum _callbackModeProcess = AnimationCallbackModeProcessEnum.Idle;
    private bool _deterministic = false;
    private bool _resetOnSave = true;
    private bool _rootMotionLocal = false;
    private NodePath _rootMotionTrack = new("");
    private NodePath _rootNode = new("..");

	[SyncVar]
	public bool AutoInit { get; set; } = true;

    /// <summary>
    /// If <c>true</c>, the <strong>AnimationMixer</strong> will be processing.
    /// </summary>
    [Editable, ScriptProperty]
    public bool Active {
        get => _active;
        set {
            _active = value;
            GDAnimationMixer.Active = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The number of possible simultaneous sounds for each of the assigned AudioStreamPlayers.
    /// For example, if this value is <c>32</c> and the animation has two audio tracks, the two <c>AudioStreamPlayer</c>s assigned can play simultaneously up to <c>32</c> voices each.
    /// </summary>
    [Editable, ScriptProperty]
    public int AudioMaxPolyphony {
        get => _audioMaxPolyphony;
        set {
            _audioMaxPolyphony = value;
            GDAnimationMixer.AudioMaxPolyphony = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Ordinarily, tracks can be set to <c>Animation.UPDATE_DISCRETE</c> to update infrequently, usually when using nearest interpolation.
    /// However, when blending with <c>Animation.UPDATE_CONTINUOUS</c> several results are considered. The <c>callback_mode_discrete</c> specify it explicitly.
    /// To make the blended results look good, it is recommended to set this to <c>ANIMATION_CALLBACK_MODE_DISCRETE_FORCE_CONTINUOUS</c> to update every frame during blending.
    /// </summary>
    [Editable, ScriptProperty]
    public AnimationCallbackModeDiscreteEnum CallbackModeDiscrete {
        get => _callbackModeDiscrete;
        set {
            _callbackModeDiscrete = value;
            GDAnimationMixer.CallbackModeDiscrete = (Godot.AnimationMixer.AnimationCallbackModeDiscrete)(int)value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The call mode used for "Call Method" tracks.
    /// </summary>
    [Editable, ScriptProperty]
    public AnimationCallbackModeMethodEnum CallbackModeMethod {
        get => _callbackModeMethod;
        set {
            _callbackModeMethod = value;
            GDAnimationMixer.CallbackModeMethod = (Godot.AnimationMixer.AnimationCallbackModeMethod)(int)value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The process notification in which to update animations.
    /// </summary>
    [Editable, ScriptProperty]
    public AnimationCallbackModeProcessEnum CallbackModeProcess {
        get => _callbackModeProcess;
        set {
            _callbackModeProcess = value;
            GDAnimationMixer.CallbackModeProcess = (Godot.AnimationMixer.AnimationCallbackModeProcess)(int)value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// If <c>true</c>, the blending uses the deterministic algorithm. The total weight is not normalized and the result is accumulated with an initial value (<c>0</c> or a <c>"RESET"</c> animation if present).
    /// If <c>false</c>, The blend does not use the deterministic algorithm. The total weight is normalized and always <c>1.0</c>.
    /// </summary>
    [Editable, ScriptProperty]
    public bool Deterministic {
        get => _deterministic;
        set {
            _deterministic = value;
            GDAnimationMixer.Deterministic = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// This is used by the editor. If set to <c>true</c>, the scene will be saved with the effects of the reset animation (the animation with the key <c>"RESET"</c>) applied as if it had been seeked to time 0.
    /// </summary>
    [Editable, ScriptProperty]
    public bool ResetOnSaveEnabled {
        get => _resetOnSave;
        set {
            _resetOnSave = value;
            GDAnimationMixer.ResetOnSave = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// If <c>true</c>, <c>get_root_motion_position()</c> value is extracted as a local translation value before blending. In other words, it is treated like the translation is done after the rotation.
    /// </summary>
    [Editable, ScriptProperty]
    public bool RootMotionLocal {
        get => _rootMotionLocal;
        set {
            _rootMotionLocal = value;
            GDAnimationMixer.RootMotionLocal = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The path to the Animation track used for root motion. Paths must be valid scene-tree paths to a node, and must be specified starting from the parent node of the node that will reproduce the animation.
    /// </summary>
    [Editable, ScriptProperty]
    public NodePath RootMotionTrack {
        get => _rootMotionTrack;
        set {
            _rootMotionTrack = value;
            GDAnimationMixer.RootMotionTrack = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The node which node path references will travel from.
    /// </summary>
    [Editable, ScriptProperty]
    public NodePath RootNode {
        get => _rootNode;
        set {
            _rootNode = value;
            GDAnimationMixer.RootNode = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Notifies when an animation finished playing.
    /// <para><strong>Note:</strong> This signal is not emitted if an animation is looping.</para>
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName> AnimationFinished { get; private set; } = new();

    /// <summary>
    /// Notifies when the animation libraries have changed.
    /// </summary>
    [ScriptProperty]
    public PTSignal AnimationLibrariesUpdated { get; private set; } = new();

    /// <summary>
    /// Notifies when an animation list is changed.
    /// </summary>
    [ScriptProperty]
    public PTSignal AnimationListChanged { get; private set; } = new();

    /// <summary>
    /// Notifies when an animation starts playing.
    /// <para><strong>Note:</strong> This signal is not emitted if an animation is looping.</para>
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName> AnimationStarted { get; private set; } = new();

    /// <summary>
    /// Notifies when the caches have been cleared, either automatically, or manually via <see cref="ClearCaches"/>.
    /// </summary>
    [ScriptProperty]
    public PTSignal CachesCleared { get; private set; } = new();

    /// <summary>
    /// Notifies when the blending result related have been applied to the target objects.
    /// </summary>
    [ScriptProperty]
    public PTSignal MixerApplied { get; private set; } = new();

    /// <summary>
    /// Notifies when the property related process have been updated.
    /// </summary>
    [ScriptProperty]
    public PTSignal MixerUpdated { get; private set; } = new();

    private void OnAnimationFinished(StringName name)
    {
        AnimationFinished.Invoke(name);
    }

    private void OnAnimationStarted(StringName name)
    {
        AnimationStarted.Invoke(name);
    }

    private void OnAnimationListChanged()
    {
        AnimationListChanged.Invoke();
    }

    private void OnAnimationLibrariesUpdated()
    {
        AnimationLibrariesUpdated.Invoke();
    }

    private void OnCachesCleared()
    {
        CachesCleared.Invoke();
    }

    private void OnMixerApplied()
    {
        MixerApplied.Invoke();
    }

    private void OnMixerUpdated()
    {
        MixerUpdated.Invoke();
    }

    public override void Init()
    {
        GDAnimationMixer.AnimationFinished += OnAnimationFinished;
        GDAnimationMixer.AnimationStarted += OnAnimationStarted;
        GDAnimationMixer.AnimationListChanged += OnAnimationListChanged;
        GDAnimationMixer.AnimationLibrariesUpdated += OnAnimationLibrariesUpdated;
        GDAnimationMixer.CachesCleared += OnCachesCleared;
        GDAnimationMixer.MixerApplied += OnMixerApplied;
        GDAnimationMixer.MixerUpdated += OnMixerUpdated;
        base.Init();
    }

	public override void PreDelete()
	{
        GDAnimationMixer.AnimationFinished -= OnAnimationFinished;
        GDAnimationMixer.AnimationStarted -= OnAnimationStarted;
        GDAnimationMixer.AnimationListChanged -= OnAnimationListChanged;
        GDAnimationMixer.AnimationLibrariesUpdated -= OnAnimationLibrariesUpdated;
        GDAnimationMixer.CachesCleared -= OnCachesCleared;
        GDAnimationMixer.MixerApplied -= OnMixerApplied;
        GDAnimationMixer.MixerUpdated -= OnMixerUpdated;
		base.PreDelete();
	}

    /// <summary>
    /// A virtual function for processing after getting a key during playback.
    /// </summary>
    [ScriptMethod]
    public virtual Variant PostProcessKeyValue(Animation animation, int track, Variant value, int objectId, int objectSubIdx)
        => new();

    /// <summary>
    /// Adds <paramref name="library"/> to the animation player, under the key <paramref name="name"/>.
    /// AnimationMixer has a global library by default with an empty string as key.
    /// </summary>
    [ScriptMethod]
    public ErrorEnum AddAnimationLibrary(PTStringName name, AnimationLibrary library)
        => (ErrorEnum)GDAnimationMixer.AddAnimationLibrary(name, library);

    /// <summary>
    /// Manually advance the animations by the specified time (in seconds).
    /// </summary>
    [ScriptMethod]
    public void Advance(float delta) => GDAnimationMixer.Advance(delta);

    /// <summary>
    /// If the animation track specified by <paramref name="name"/> has an option <c>Animation.UPDATE_CAPTURE</c>, stores current values of the objects indicated by the track path as a cache.
    /// After this it will interpolate with current animation blending result during the playback process for the time specified by <paramref name="duration"/>.
    /// </summary>
    [ScriptMethod]
    public void Capture(PTStringName name, float duration, TransitionTypeEnum transType = TransitionTypeEnum.Linear, EaseTypeEnum easeType = EaseTypeEnum.In)
        => GDAnimationMixer.Capture(name, duration, (Godot.Tween.TransitionType)(int)transType, (Godot.Tween.EaseType)(int)easeType);

    /// <summary>
    /// <strong>AnimationMixer</strong> caches animated nodes. It may not notice if a node disappears; <see cref="ClearCaches"/> forces it to update the cache again.
    /// </summary>
    [ScriptMethod]
    public void ClearCaches() => GDAnimationMixer.ClearCaches();

    /// <summary>
    /// Returns the key of <paramref name="animation"/> or an empty <c>StringName</c> if not found.
    /// </summary>
    [ScriptMethod]
    public PTStringName FindAnimation(Animation animation) => GDAnimationMixer.FindAnimation(animation);

    /// <summary>
    /// Returns the key for the <c>AnimationLibrary</c> that contains <paramref name="animation"/> or an empty <c>StringName</c> if not found.
    /// </summary>
    [ScriptMethod]
    public PTStringName FindAnimationLibrary(Animation animation) => GDAnimationMixer.FindAnimationLibrary(animation);

    /// <summary>
    /// Returns the <c>Animation</c> with the key <paramref name="name"/>. If the animation does not exist, <c>null</c> is returned and an error is logged.
    /// </summary>
    [ScriptMethod]
    public Animation? GetAnimation(PTStringName name) => GDAnimationMixer.GetAnimation(name);

    /// <summary>
    /// Returns the first <c>AnimationLibrary</c> with key <paramref name="name"/> or <c>null</c> if not found.
    /// To get the <strong>AnimationMixer</strong>'s global animation library, use <c>get_animation_library("")</c>.
    /// </summary>
    [ScriptMethod]
    public AnimationLibrary? GetAnimationLibrary(PTStringName name) => GDAnimationMixer.GetAnimationLibrary(name);

    /// <summary>
    /// Returns the list of stored library keys.
    /// </summary>
    [ScriptMethod]
    public PTStringName[] GetAnimationLibraryList()
     {
        var array = GDAnimationMixer.GetAnimationLibraryList();
        var result = new PTStringName[array.Count];
        for (int i = 0; i < array.Count; i++)
        {
            result[i] = (PTStringName)array[i];
        }
        return result;
    }

    /// <summary>
    /// Returns the list of stored animation keys.
    /// </summary>
    [ScriptMethod]
    public string[] GetAnimationList() => GDAnimationMixer.GetAnimationList();

    /// <summary>
    /// Retrieve the motion delta of position with the <see cref="RootMotionTrack"/> as a <c>Vector3</c> that can be used elsewhere.
    /// If <see cref="RootMotionTrack"/> is not a path to a track of type <c>Animation.TYPE_POSITION_3D</c>, returns <c>Vector3(0, 0, 0)</c>.
    /// </summary>
    [ScriptMethod]
    public Vector3 GetRootMotionPosition() => GDAnimationMixer.GetRootMotionPosition();

    /// <summary>
    /// Retrieve the blended value of the position tracks with the <see cref="RootMotionTrack"/> as a <c>Vector3</c> that can be used elsewhere.
    /// This is useful in cases where you want to respect the initial key values of the animation.
    /// </summary>
    [ScriptMethod]
    public Vector3 GetRootMotionPositionAccumulator() => GDAnimationMixer.GetRootMotionPositionAccumulator();

    /// <summary>
    /// Retrieve the motion delta of rotation with the <see cref="RootMotionTrack"/> as a <c>Quaternion</c> that can be used elsewhere.
    /// If <see cref="RootMotionTrack"/> is not a path to a track of type <c>Animation.TYPE_ROTATION_3D</c>, returns <c>Quaternion(0, 0, 0, 1)</c>.
    /// </summary>
    [ScriptMethod]
    public Quaternion GetRootMotionRotation() => GDAnimationMixer.GetRootMotionRotation();

    /// <summary>
    /// Retrieve the blended value of the rotation tracks with the <see cref="RootMotionTrack"/> as a <c>Quaternion</c> that can be used elsewhere.
    /// This is necessary to apply the root motion position correctly, taking rotation into account.
    /// </summary>
    [ScriptMethod]
    public Quaternion GetRootMotionRotationAccumulator() => GDAnimationMixer.GetRootMotionRotationAccumulator();

    /// <summary>
    /// Retrieve the motion delta of scale with the <see cref="RootMotionTrack"/> as a <c>Vector3</c> that can be used elsewhere.
    /// If <see cref="RootMotionTrack"/> is not a path to a track of type <c>Animation.TYPE_SCALE_3D</c>, returns <c>Vector3(0, 0, 0)</c>.
    /// </summary>
    [ScriptMethod]
    public Vector3 GetRootMotionScale() => GDAnimationMixer.GetRootMotionScale();

    /// <summary>
    /// Retrieve the blended value of the scale tracks with the <see cref="RootMotionTrack"/> as a <c>Vector3</c> that can be used elsewhere.
    /// </summary>
    [ScriptMethod]
    public Vector3 GetRootMotionScaleAccumulator() => GDAnimationMixer.GetRootMotionScaleAccumulator();

    /// <summary>
    /// Returns <c>true</c> if the <strong>AnimationMixer</strong> stores an <c>Animation</c> with key <paramref name="name"/>.
    /// </summary>
    [ScriptMethod]
    public bool HasAnimation(PTStringName name) => GDAnimationMixer.HasAnimation(name);

    /// <summary>
    /// Returns <c>true</c> if the <strong>AnimationMixer</strong> stores an <c>AnimationLibrary</c> with key <paramref name="name"/>.
    /// </summary>
    [ScriptMethod]
    public bool HasAnimationLibrary(PTStringName name) => GDAnimationMixer.HasAnimationLibrary(name);

    /// <summary>
    /// Removes the <c>AnimationLibrary</c> associated with the key <paramref name="name"/>.
    /// </summary>
    [ScriptMethod]
    public void RemoveAnimationLibrary(PTStringName name) => GDAnimationMixer.RemoveAnimationLibrary(name);

    /// <summary>
    /// Moves the <c>AnimationLibrary</c> associated with the key <paramref name="name"/> to the key <paramref name="newname"/>.
    /// </summary>
    [ScriptMethod]
    public void RenameAnimationLibrary(PTStringName name, PTStringName newname) => GDAnimationMixer.RenameAnimationLibrary(name, newname);
}