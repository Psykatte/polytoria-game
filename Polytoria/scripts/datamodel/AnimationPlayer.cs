// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Enums;
using Polytoria.Scripting;
using Polytoria.Scripting.Datatypes;
using Polytoria.Shared;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationPlayer is an anti-corruption layer for interfacing with Godot.AnimationPlayer.

/// <summary>
/// An animation player is used for general-purpose playback of animations. It contains a dictionary of <see cref="AnimationLibrary"/> resources and custom blend times between animation transitions.
/// <para>Some methods and properties use a single key to reference an animation directly. These keys are formatted as the key for the library, followed by a forward slash, then the key for the animation within the library, for example <c>"movement/run"</c>. If the library's key is an empty string (known as the default library), the forward slash is omitted, being the same key used by the library.</para>
/// <para><strong>AnimationPlayer</strong> is better-suited than <see cref="Tween"/> for more complex animations, for example ones with non-trivial timings. It can also be used over <see cref="Tween"/> if the animation track editor is more convenient than doing it in code.</para>
/// <para>Updating the target properties of animations occurs at the process frame.</para>
/// </summary>
[Instantiable]
public partial class AnimationPlayer : AnimationMixer
{
    internal Godot.AnimationPlayer GDAnimationPlayer = null!;
    protected override Godot.AnimationMixer GDAnimationMixer => GDAnimationPlayer;

    private PTStringName? _assignedAnimation = null;
    private PTStringName _autoplay = new();
    private PTStringName _currentAnimation = new();
    private bool _movieQuitOnFinish = false;
    private bool _playbackAutoCapture = true;
    private float _playbackAutoCaptureDuration = -1.0f;
    private EaseTypeEnum _playbackAutoCaptureEaseType = EaseTypeEnum.In;
    private TransitionTypeEnum _playbackAutoCaptureTransitionType = TransitionTypeEnum.Linear;
    private float _playbackDefaultBlendTime = 0.0f;
    private float _speedScale = 1.0f;

    /// <summary>
    /// Emitted when a queued animation plays after the previous animation finished. See also <see cref="Queue"/>.
    /// <para><strong>Note:</strong> The signal is not emitted when the animation is changed via <see cref="Play"/> or by an <c>AnimationTree</c>.</para>
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName, PTStringName> AnimationChanged { get; private set; } = new();

    /// <summary>
    /// Emitted when <see cref="CurrentAnimation"/> changes.
    /// </summary>
    [ScriptProperty]
    public PTSignal<PTStringName> CurrentAnimationChanged { get; private set; } = new();

    private void OnAnimationChanged(StringName oldName, StringName newName)
    {
        AnimationChanged.Invoke(oldName, newName);
    }

    private void OnCurrentAnimationChanged(StringName name)
    {
        CurrentAnimationChanged.Invoke(name);
    }
    
    public override Node CreateGDNode() => Globals.LoadNetworkedObjectScene(ClassName)!;

    public override void Init()
    {
        GDAnimationPlayer = (Godot.AnimationPlayer)GDNode;
        GDAnimationPlayer.AnimationChanged += OnAnimationChanged;
        GDAnimationPlayer.CurrentAnimationChanged += OnCurrentAnimationChanged;
        base.Init();
    }

	public override void PreDelete()
	{
        GDAnimationPlayer.AnimationChanged -= OnAnimationChanged;
        GDAnimationPlayer.CurrentAnimationChanged -= OnCurrentAnimationChanged;
		base.PreDelete();
	}

    /// <summary>
    /// If playing, the current animation's key, otherwise, the animation last played.
    /// When set, this changes the animation, but will not play it unless already playing.
    /// See also <see cref="CurrentAnimation"/>.
    /// </summary>
    [Editable, ScriptProperty]
    public PTStringName? AssignedAnimation {
        get => _assignedAnimation;
        set {
            _assignedAnimation = value;
            if (value != null) GDAnimationPlayer.AssignedAnimation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The key of the animation to play when the scene loads.
    /// </summary>
    [Editable, ScriptProperty]
    public PTStringName Autoplay {
        get => _autoplay;
        set {
            _autoplay = value;
            GDAnimationPlayer.Autoplay = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The key of the currently playing animation. If no animation is playing, the property's value is an empty string.
    /// Changing this value does not restart the animation. See <see cref="Play"/> for more information on playing animations.
    /// <para><strong>Note:</strong> While this property appears in the Inspector, it's not meant to be edited, and it's not saved in the scene.
    /// This property is mainly used to get the currently playing animation, and internally for animation playback tracks.</para>
    /// </summary>
    [ScriptProperty]
    public PTStringName CurrentAnimation {
        get => _currentAnimation;
    }

    /// <summary>
    /// The length (in seconds) of the currently playing animation.
    /// </summary>
    [ScriptProperty]
    public float? CurrentAnimationLength {
        get {
            var val = GDAnimationPlayer?.CurrentAnimationLength;
            return val.HasValue ? (float)val.Value : null;
        }
    }

    /// <summary>
    /// The position (in seconds) of the currently playing animation.
    /// </summary>
    [ScriptProperty]
    public float? CurrentAnimationPosition {
        get {
            var val = GDAnimationPlayer?.CurrentAnimationPosition;
            return val.HasValue ? (float)val.Value : null;
        }
    }

    /// <summary>
    /// If <c>true</c> and the engine is running in Movie Maker mode (see <c>MovieWriter</c>), exits the engine with <c>SceneTree.Quit()</c>
    /// as soon as an animation is done playing in this <c>Polytoria.Datamodel.AnimationPlayer</c>. A message is printed when the engine quits for this reason.
    /// <para><strong>Note:</strong> This obeys the same logic as the <c>AnimationMixer.AnimationFinished</c> signal, so it will not quit the engine if the animation is set to be looping.</para>
    /// </summary>
    [Editable, ScriptProperty]
    public bool MovieQuitOnFinishEnabled {
        get => _movieQuitOnFinish;
        set {
            _movieQuitOnFinish = value;
            GDAnimationPlayer.MovieQuitOnFinish = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// If <c>true</c>, performs <c>AnimationMixer.Capture()</c> before playback automatically.
    /// This means just <see cref="PlayWithCapture"/> is executed with default arguments instead of <see cref="Play"/>.
    /// <para><strong>Note:</strong> Capture interpolation is only performed if the animation contains a capture track. See also <c>Animation.UpdateCapture</c>.</para>
    /// </summary>
    [Editable, ScriptProperty]
    public bool PlaybackAutoCapture {
        get => _playbackAutoCapture;
        set {
            _playbackAutoCapture = value;
            GDAnimationPlayer.PlaybackAutoCapture = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Duration for auto capture. If negative, the duration is set to the interval between the current position and the first key.
    /// See also <see cref="PlayWithCapture"/> and <c>AnimationMixer.Capture()</c>.
    /// </summary>
    [Editable, ScriptProperty]
    public float PlaybackAutoCaptureDuration {
        get => _playbackAutoCaptureDuration;
        set {
            _playbackAutoCaptureDuration = value;
            GDAnimationPlayer.PlaybackAutoCaptureDuration = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The transition type of the capture interpolation. See also <c>TransitionType</c>.
    /// </summary>
    [Editable, ScriptProperty]
    public TransitionTypeEnum PlaybackAutoCaptureTransitionType {
        get => _playbackAutoCaptureTransitionType;
        set {
            _playbackAutoCaptureTransitionType = value;
            GDAnimationPlayer.PlaybackAutoCaptureTransitionType = (Godot.Tween.TransitionType)(int)value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The ease type of the capture interpolation. See also <c>EaseType</c>.
    /// </summary>
    [Editable, ScriptProperty]
    public EaseTypeEnum PlaybackAutoCaptureEaseType {
        get => _playbackAutoCaptureEaseType;
        set {
            _playbackAutoCaptureEaseType = value;
            GDAnimationPlayer.PlaybackAutoCaptureEaseType = (Godot.Tween.EaseType)(int)value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The default time in which to blend animations. Ranges from 0 to 4096 with 0.01 precision.
    /// </summary>
    [Editable, ScriptProperty]
    public float PlaybackDefaultBlendTime {
        get => _playbackDefaultBlendTime;
        set {
            _playbackDefaultBlendTime = value;
            GDAnimationPlayer.PlaybackDefaultBlendTime = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The speed scaling ratio. For example, if this value is <c>1</c>, then the animation plays at normal speed.
    /// If it's <c>0.5</c>, then it plays at half speed. If it's <c>2</c>, then it plays at double speed.
    /// If set to a negative value, the animation is played in reverse. If set to <c>0</c>, the animation will not advance.
    /// </summary>
    [Editable, ScriptProperty]
    public float SpeedScale {
        get => _speedScale;
        set {
            _speedScale = value;
            GDAnimationPlayer.SpeedScale = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Returns the key of the animation which is queued to play after the <paramref name="animationFrom"/> animation.
    /// </summary>
    [ScriptMethod]
    public PTStringName AnimationGetNext(PTStringName animationFrom) => GDAnimationPlayer.AnimationGetNext(animationFrom);

    /// <summary>
    /// Triggers the <paramref name="animationTo"/> animation when the <paramref name="animationFrom"/> animation completes.
    /// </summary>
    [ScriptMethod]
    public void AnimationSetNext(PTStringName animationFrom, PTStringName animationTo) => GDAnimationPlayer.AnimationSetNext(animationFrom, animationTo);

    /// <summary>
    /// Clears all queued, unplayed animations.
    /// </summary>
    [ScriptMethod]
    public void ClearQueue() => GDAnimationPlayer.ClearQueue();

    /// <summary>
    /// Returns the blend time (in seconds) between two animations, referenced by their keys.
    /// </summary>
    [ScriptMethod]
    public double GetBlendTime(PTStringName animationFrom, PTStringName animationTo) => GDAnimationPlayer.GetBlendTime(animationFrom, animationTo);

    /// <summary>
    /// Returns the actual playing speed of current animation or <c>0</c> if not playing.
    /// This speed is the <see cref="SpeedScale"/> property multiplied by <c>customSpeed</c> argument specified when calling the <see cref="Play"/> method.
    /// Returns a negative value if the current animation is playing backwards.
    /// </summary>
    [ScriptMethod]
    public float GetPlayingSpeed() => GDAnimationPlayer.GetPlayingSpeed();

    /// <summary>
    /// Returns a list of the animation keys that are currently queued to play.
    /// </summary>
    [ScriptMethod]
    public PTStringName[] GetQueue()
    {
        var array = GDAnimationPlayer.GetQueue();
        var result = new PTStringName[array.Count];
        for (int i = 0; i < array.Count; i++)
        {
            result[i] = (PTStringName)array[i];
        }
        return result;
    }

    /// <summary>
    /// Returns the end time of the section currently being played.
    /// </summary>
    [ScriptMethod]
    public double GetSectionEndTime() => GDAnimationPlayer.GetSectionEndTime();

    /// <summary>
    /// Returns the start time of the section currently being played.
    /// </summary>
    [ScriptMethod]
    public double GetSectionStartTime() => GDAnimationPlayer.GetSectionStartTime();

    /// <summary>
    /// Returns <c>true</c> if an animation is currently playing with a section.
    /// </summary>
    [ScriptMethod]
    public bool HasSection() => GDAnimationPlayer.HasSection();

    /// <summary>
    /// Returns <c>true</c> if the an animation is currently active. An animation is active if it was played by calling <see cref="Play"/>
    /// and was not finished yet, or was stopped by calling <see cref="Stop"/>.
    /// This can be used to check whether an animation is currently paused or stopped.
    /// </summary>
    [ScriptMethod]
    public bool IsAnimationActive() => GDAnimationPlayer.IsAnimationActive();

    /// <summary>
    /// Returns <c>true</c> if an animation is currently playing (even if <see cref="SpeedScale"/> and/or <c>customSpeed</c> are <c>0</c>).
    /// </summary>
    [ScriptMethod]
    public bool IsPlaying() => GDAnimationPlayer.IsPlaying();

    /// <summary>
    /// Pauses the currently playing animation. The <see cref="CurrentAnimationPosition"/> will be kept and calling <see cref="Play"/>
    /// or <see cref="PlayBackwards"/> without arguments or with the same animation name as <see cref="AssignedAnimation"/> will resume the animation.
    /// See also <see cref="Stop"/>.
    /// </summary>
    [ScriptMethod]
    public void Pause() => GDAnimationPlayer.Pause();

    /// <summary>
    /// Plays the animation with key <paramref name="name"/>. Custom blend times and speed can be set.
    /// The <paramref name="fromEnd"/> option only affects when switching to a new animation track, or if the same track but at the start or end.
    /// It does not affect resuming playback that was paused in the middle of an animation.
    /// If <paramref name="customSpeed"/> is negative and <paramref name="fromEnd"/> is <c>true</c>, the animation will play backwards (equivalent to <see cref="PlayBackwards"/>).
    /// The <c>Polytoria.Datamodel.AnimationPlayer</c> keeps track of its current or last played animation with <see cref="AssignedAnimation"/>.
    /// If this method is called with that same animation <paramref name="name"/>, or with no <paramref name="name"/> parameter,
    /// the assigned animation will resume playing if it was paused.
    /// <para><strong>Note:</strong> The animation will be updated the next time the <c>Polytoria.Datamodel.AnimationPlayer</c> is processed.
    /// If other variables are updated at the same time this is called, they may be updated too early. To perform the update immediately, call <c>Advance(0)</c>.</para>
    /// </summary>
    [ScriptMethod]
    public void Play(PTStringName? name = null, double customBlend = -1, float customSpeed = 1, bool fromEnd = false)
        => GDAnimationPlayer.Play(name, customBlend, customSpeed, fromEnd);

    /// <summary>
    /// Plays the animation with key <paramref name="name"/> in reverse.
    /// This method is a shorthand for <see cref="Play"/> with <c>customSpeed = -1.0</c> and <c>fromEnd = true</c>.
    /// </summary>
    [ScriptMethod]
    public void PlayBackwards(PTStringName? name = null, double customBlend = -1)
        => GDAnimationPlayer.PlayBackwards(name, customBlend);

    /// <summary>
    /// Plays the animation with key <paramref name="name"/> and the section starting from <paramref name="startTime"/> and ending on <paramref name="endTime"/>.
    /// Setting <paramref name="startTime"/> to a value outside the range of the animation means the start of the animation will be used instead,
    /// and setting <paramref name="endTime"/> to a value outside the range of the animation means the end of the animation will be used instead.
    /// <paramref name="startTime"/> cannot be equal to <paramref name="endTime"/>.
    /// </summary>
    [ScriptMethod]
    public void PlaySection(PTStringName? name = null, double startTime = -1, double endTime = -1, double customBlend = -1, float customSpeed = 1, bool fromEnd = false)
        => GDAnimationPlayer.PlaySection(name, startTime, endTime, customBlend, customSpeed, fromEnd);

    /// <summary>
    /// Plays the animation with key <paramref name="name"/> and the section starting from <paramref name="startTime"/> and ending on <paramref name="endTime"/> in reverse.
    /// This method is a shorthand for <see cref="PlaySection"/> with <c>customSpeed = -1.0</c> and <c>fromEnd = true</c>.
    /// </summary>
    [ScriptMethod]
    public void PlaySectionBackwards(PTStringName? name = null, double startTime = -1, double endTime = -1, double customBlend = -1)
        => GDAnimationPlayer.PlaySectionBackwards(name, startTime, endTime, customBlend);

    /// <summary>
    /// Plays the animation with key <paramref name="name"/> and the section starting from <paramref name="startMarker"/> and ending on <paramref name="endMarker"/>.
    /// If the start marker is empty, the section starts from the beginning of the animation.
    /// If the end marker is empty, the section ends on the end of the animation.
    /// </summary>
    [ScriptMethod]
    public void PlaySectionWithMarkers(PTStringName? name = null, StringName? startMarker = null, StringName? endMarker = null, double customBlend = -1, float customSpeed = 1, bool fromEnd = false)
        => GDAnimationPlayer.PlaySectionWithMarkers(name, startMarker, endMarker, customBlend, customSpeed, fromEnd);

    /// <summary>
    /// Plays the animation with key <paramref name="name"/> and the section starting from <paramref name="startMarker"/> and ending on <paramref name="endMarker"/> in reverse.
    /// This method is a shorthand for <see cref="PlaySectionWithMarkers"/> with <c>customSpeed = -1.0</c> and <c>fromEnd = true</c>.
    /// </summary>
    [ScriptMethod]
    public void PlaySectionWithMarkersBackwards(PTStringName? name = null, StringName? startMarker = null, StringName? endMarker = null, double customBlend = -1)
        => GDAnimationPlayer.PlaySectionWithMarkersBackwards(name, startMarker, endMarker, customBlend);

    /// <summary>
    /// You can use this method to use more detailed options for capture than those performed by <see cref="PlaybackAutoCapture"/>.
    /// When <see cref="PlaybackAutoCapture"/> is <c>false</c>, this method is almost the same as calling <c>Capture()</c> followed by <see cref="Play"/>.
    /// If <paramref name="name"/> is blank, it specifies <see cref="AssignedAnimation"/>.
    /// If <paramref name="duration"/> is a negative value, the duration is set to the interval between the current position and the first key.
    /// When <paramref name="fromEnd"/> is <c>true</c>, uses the interval between the current position and the last key instead.
    /// <para><strong>Note:</strong> The <paramref name="duration"/> takes <see cref="SpeedScale"/> into account, but <paramref name="customSpeed"/> does not.</para>
    /// </summary>
    [ScriptMethod]
    public void PlayWithCapture(PTStringName? name = null, double duration = -1, double customBlend = -1, float customSpeed = 1, bool fromEnd = false, TransitionTypeEnum transType = TransitionTypeEnum.Linear, EaseTypeEnum easeType = EaseTypeEnum.In)
        => GDAnimationPlayer.PlayWithCapture(name, duration, customBlend, customSpeed, fromEnd, (Godot.Tween.TransitionType)(int)transType, (Godot.Tween.EaseType)(int)easeType);

    /// <summary>
    /// Queues an animation for playback once the current animation and all previously queued animations are done.
    /// <para><strong>Note:</strong> If a looped animation is currently playing, the queued animation will never play unless the looped animation is stopped somehow.</para>
    /// </summary>
    [ScriptMethod]
    public void Queue(PTStringName name) => GDAnimationPlayer.Queue(name);

    /// <summary>
    /// Resets the current section. Does nothing if a section has not been set.
    /// </summary>
    [ScriptMethod]
    public void ResetSection() => GDAnimationPlayer.ResetSection();

    /// <summary>
    /// Seeks the animation to the <paramref name="seconds"/> point in time (in seconds).
    /// If <paramref name="update"/> is <c>true</c>, the animation updates too, otherwise it updates at process time.
    /// Events between the current frame and <paramref name="seconds"/> are skipped.
    /// If <paramref name="updateOnly"/> is <c>true</c>, the method / audio / animation playback tracks will not be processed.
    /// <para><strong>Note:</strong> Seeking to the end of the animation doesn't emit <c>AnimationMixer.AnimationFinished</c>.
    /// If you want to skip animation and emit the signal, use <c>AnimationMixer.Advance()</c>.</para>
    /// </summary>
    [ScriptMethod]
    public void Seek(double seconds, bool update = false, bool updateOnly = false)
        => GDAnimationPlayer.Seek(seconds, update, updateOnly);

    /// <summary>
    /// Specifies a blend time (in seconds) between two animations, referenced by their keys.
    /// </summary>
    [ScriptMethod]
    public void SetBlendTime(StringName animationFrom, StringName animationTo, double sec)
        => GDAnimationPlayer.SetBlendTime(animationFrom, animationTo, sec);

    /// <summary>
    /// Changes the start and end times of the section being played. The current playback position will be clamped within the new section.
    /// See also <see cref="PlaySection"/>.
    /// </summary>
    [ScriptMethod]
    public void SetSection(double startTime = -1, double endTime = -1)
        => GDAnimationPlayer.SetSection(startTime, endTime);

    /// <summary>
    /// Changes the start and end markers of the section being played. The current playback position will be clamped within the new section.
    /// See also <see cref="PlaySectionWithMarkers"/>.
    /// If the argument is empty, the section uses the beginning or end of the animation. If both are empty, it means that the section is not set.
    /// </summary>
    [ScriptMethod]
    public void SetSectionWithMarkers(StringName? startMarker = null, StringName? endMarker = null)
        => GDAnimationPlayer.SetSectionWithMarkers(startMarker, endMarker);

    /// <summary>
    /// Stops the currently playing animation. The animation position is reset to <c>0</c> and the <c>customSpeed</c> is reset to <c>1.0</c>.
    /// See also <see cref="Pause"/>.
    /// If <paramref name="keepState"/> is <c>true</c>, the animation state is not updated visually.
    /// <para><strong>Note:</strong> The method / audio / animation playback tracks will not be processed by this method.</para>
    /// </summary>
    [ScriptMethod]
    public void Stop(bool keepState = false) => GDAnimationPlayer.Stop(keepState);
}