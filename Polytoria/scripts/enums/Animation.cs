// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Enums;

/// <summary>
/// Process animation during physics frames (see <c>Node.NOTIFICATION_INTERNAL_PHYSICS_PROCESS</c>). 
/// This is especially useful when animating physics bodies.
/// </summary>
[ScriptEnum]
public enum AnimationCallbackModeProcessEnum
{
    /// <summary>
    /// Process animation during physics frames (see <see cref="Godot.Node.NotificationInternalPhysicsProcess"/>). 
    /// This is especially useful when animating physics bodies.
    /// </summary>
    Physics = 0,

    /// <summary>
    /// Process animation during process frames (see <see cref="Godot.Node.NotificationInternalProcess"/>).
    /// </summary>
    Idle = 1,

    /// <summary>
    /// Do not process animation. Use <see cref="Polytoria.Datamodel.AnimationMixer.Advance"/> to process the animation manually.
    /// </summary>
    Manual = 2
}

/// <summary>
/// Controls how method calls are handled during animation playback.
/// </summary>
[ScriptEnum]
public enum AnimationCallbackModeMethodEnum
{
    /// <summary>
    /// Batch method calls during the animation process, then do the calls after events are processed. 
    /// This avoids bugs involving deleting nodes or modifying the AnimationPlayer while playing.
    /// </summary>
    Deferred = 0,

    /// <summary>
    /// Make method calls immediately when reached in the animation.
    /// </summary>
    Immediate = 1
}

/// <summary>
/// Controls how discrete track values are blended with continuous or capture track values.
/// </summary>
[ScriptEnum]
public enum AnimationCallbackModeDiscreteEnum
{
    /// <summary>
    /// An <see cref="UpdateModeEnum.Discrete"/> track value takes precedence when blending
    /// <see cref="UpdateModeEnum.Continuous"/> or <see cref="UpdateModeEnum.Capture"/> track values
    /// and <see cref="UpdateModeEnum.Discrete"/> track values.
    /// </summary>
    Dominant = 0,

    /// <summary>
    /// An <see cref="UpdateModeEnum.Continuous"/> or <see cref="UpdateModeEnum.Capture"/> track value takes precedence
    /// when blending the <see cref="UpdateModeEnum.Continuous"/> or <see cref="UpdateModeEnum.Capture"/> track values
    /// and the <see cref="UpdateModeEnum.Discrete"/> track values. This is the default behavior for <see cref="Polytoria.Datamodel.AnimationPlayer"/>.
    /// </summary>
    Recessive = 1,

    /// <summary>
    /// Always treat the <see cref="UpdateModeEnum.Discrete"/> track value as <see cref="UpdateModeEnum.Continuous"/>
    /// with <see cref="InterpolationTypeEnum.Nearest"/>. This is the default behavior for <see cref="Polytoria.Datamodel.AnimationTree"/>.
    /// <para>If a value track has un-interpolatable type key values, it is internally converted to use
    /// <see cref="Recessive"/> with <see cref="UpdateModeEnum.Discrete"/>.</para>
    /// </summary>
    ForceContinuous = 2
}

/// <summary>
/// Defines the easing behavior for interpolation.
/// </summary>
[ScriptEnum]
public enum EaseTypeEnum
{
    /// <summary>
    /// The interpolation starts slowly and speeds up towards the end.
    /// </summary>
    In = 0,

    /// <summary>
    /// The interpolation starts quickly and slows down towards the end.
    /// </summary>
    Out = 1,

    /// <summary>
    /// A combination of <see cref="In"/> and <see cref="Out"/>. 
    /// The interpolation is slowest at both ends.
    /// </summary>
    InOut = 2,

    /// <summary>
    /// A combination of <see cref="In"/> and <see cref="Out"/>. 
    /// The interpolation is fastest at both ends.
    /// </summary>
    OutIn = 3
}

/// <summary>
/// Defines the search mode for finding keys in animations.
/// </summary>
[ScriptEnum]
public enum FindModeEnum
{
    /// <summary>
    /// Finds the nearest time key.
    /// </summary>
    Nearest = 0,

    /// <summary>
    /// Finds only the key with approximating the time.
    /// </summary>
    Approx = 1,

    /// <summary>
    /// Finds only the key with matching the time.
    /// </summary>
    Exact = 2
}

/// <summary>
/// Defines the interpolation method for animation tracks.
/// </summary>
[ScriptEnum]
public enum InterpolationTypeEnum
{
    /// <summary>
    /// No interpolation (nearest value).
    /// </summary>
    Nearest = 0,

    /// <summary>
    /// Linear interpolation.
    /// </summary>
    Linear = 1,

    /// <summary>
    /// Cubic interpolation. This looks smoother than linear interpolation, but is more expensive to interpolate. 
    /// Stick to <see cref="Linear"/> for complex 3D animations imported from external software, 
    /// even if it requires using a higher animation framerate in return.
    /// </summary>
    Cubic = 2,

    /// <summary>
    /// Linear interpolation with shortest path rotation.
    /// <para><strong>Note:</strong> The result value is always normalized and may not match the key value.</para>
    /// </summary>
    LinearAngle = 3,

    /// <summary>
    /// Cubic interpolation with shortest path rotation.
    /// <para><strong>Note:</strong> The result value is always normalized and may not match the key value.</para>
    /// </summary>
    CubicAngle = 4
}

/// <summary>
/// Flags indicating loop state during animation playback.
/// </summary>
[ScriptEnum]
public enum LoopedFlagEnum
{
    /// <summary>
    /// This flag indicates that the animation proceeds without any looping.
    /// </summary>
    None = 0,

    /// <summary>
    /// This flag indicates that the animation has reached the end of the animation and just after loop processed.
    /// </summary>
    End = 1,

    /// <summary>
    /// This flag indicates that the animation has reached the start of the animation and just after loop processed.
    /// </summary>
    Start = 2
}

/// <summary>
/// Defines the loop mode for animations.
/// </summary>
[ScriptEnum]
public enum LoopModeEnum
{
    /// <summary>
    /// At both ends of the animation, the animation will stop playing.
    /// </summary>
    None = 0,

    /// <summary>
    /// At both ends of the animation, the animation will be repeated without changing the playback direction.
    /// </summary>
    Linear = 1,

    /// <summary>
    /// Repeats playback and reverse playback at both ends of the animation.
    /// </summary>
    PingPong = 2
}

/// <summary>
/// Defines the type of animation track.
/// </summary>
[ScriptEnum]
public enum TrackTypeEnum
{
    /// <summary>
    /// Value tracks set values in node properties, but only those which can be interpolated. 
    /// For 3D position/rotation/scale, using the dedicated <see cref="Position3D"/>, 
    /// <see cref="Rotation3D"/> and <see cref="Scale3D"/> track types instead of <see cref="Value"/> 
    /// is recommended for performance reasons.
    /// </summary>
    Value = 0,

    /// <summary>
    /// 3D position track (values are stored in <see cref="Godot.Vector3"/>s).
    /// </summary>
    Position3D = 1,

    /// <summary>
    /// 3D rotation track (values are stored in <see cref="Godot.Quaternion"/>s).
    /// </summary>
    Rotation3D = 2,

    /// <summary>
    /// 3D scale track (values are stored in <see cref="Godot.Vector3"/>s).
    /// </summary>
    Scale3D = 3,

    /// <summary>
    /// Blend shape track.
    /// </summary>
    BlendShape = 4,

    /// <summary>
    /// Method tracks call functions with given arguments per key.
    /// </summary>
    Method = 5,

    /// <summary>
    /// Bezier tracks are used to interpolate a value using custom curves. 
    /// They can also be used to animate sub-properties of vectors and colors (e.g. alpha value of a <see cref="Godot.Color"/>).
    /// </summary>
    Bezier = 6,

    /// <summary>
    /// Audio tracks are used to play an audio stream with either type of <see cref="Godot.AudioStreamPlayer"/>. 
    /// The stream can be trimmed and previewed in the animation.
    /// </summary>
    Audio = 7,

    /// <summary>
    /// Animation tracks play animations in other <see cref="Godot.AnimationPlayer"/> nodes.
    /// </summary>
    Animation = 8
}

/// <summary>
/// Defines the transition function used for interpolation.
/// </summary>
[ScriptEnum]
public enum TransitionTypeEnum
{
    /// <summary>
    /// The animation is interpolated linearly.
    /// </summary>
    Linear = 0,

    /// <summary>
    /// The animation is interpolated using a sine function.
    /// </summary>
    Sine = 1,

    /// <summary>
    /// The animation is interpolated with a quintic (to the power of 5) function.
    /// </summary>
    Quint = 2,

    /// <summary>
    /// The animation is interpolated with a quartic (to the power of 4) function.
    /// </summary>
    Quart = 3,

    /// <summary>
    /// The animation is interpolated with a quadratic (to the power of 2) function.
    /// </summary>
    Quad = 4,

    /// <summary>
    /// The animation is interpolated with an exponential (to the power of x) function.
    /// </summary>
    Expo = 5,

    /// <summary>
    /// The animation is interpolated with elasticity, wiggling around the edges.
    /// </summary>
    Elastic = 6,

    /// <summary>
    /// The animation is interpolated with a cubic (to the power of 3) function.
    /// </summary>
    Cubic = 7,

    /// <summary>
    /// The animation is interpolated with a function using square roots.
    /// </summary>
    Circ = 8,

    /// <summary>
    /// The animation is interpolated by bouncing at the end.
    /// </summary>
    Bounce = 9,

    /// <summary>
    /// The animation is interpolated backing out at ends.
    /// </summary>
    Back = 10,

    /// <summary>
    /// The animation is interpolated like a spring towards the end.
    /// </summary>
    Spring = 11
}

/// <summary>
/// Defines how a Tween behaves when the scene tree is paused.
/// </summary>
[ScriptEnum]
public enum TweenPauseModeEnum
{
    /// <summary>
    /// If the <see cref="Polytoria.Datamodel.Tween"/> has a bound node, it will process when that node can process
    /// (see <see cref="Godot.Node.ProcessModeEnum"/>). Otherwise it's the same as <see cref="Stop"/>.
    /// </summary>
    Bound = 0,

    /// <summary>
    /// If <see cref="Polytoria.Datamodel.SceneTree"/> is paused, the <see cref="Polytoria.Datamodel.Tween"/> will also pause.
    /// </summary>
    Stop = 1,

    /// <summary>
    /// The <see cref="Polytoria.Datamodel.Tween"/> will process regardless of whether <see cref="Polytoria.Datamodel.SceneTree"/> is paused.
    /// </summary>
    Process = 2
}

/// <summary>
/// Defines the update mode for Tween objects.
/// </summary>
[ScriptEnum]
public enum TweenProcessModeEnum
{
    /// <summary>
    /// The <see cref="Polytoria.Datamodel.Tween"/> updates after each physics frame (see <see cref="Node._PhysicsProcess"/>).
    /// </summary>
    Physics = 0,

    /// <summary>
    /// The <see cref="Polytoria.Datamodel.Tween"/> updates after each process frame (see <see cref="Node._Process"/>).
    /// </summary>
    Idle = 1
}

/// <summary>
/// Defines the update mode for animation tracks.
/// </summary>
[ScriptEnum]
public enum UpdateModeEnum
{
    /// <summary>
    /// Update between keyframes and hold the value.
    /// </summary>
    Continuous = 0,

    /// <summary>
    /// Update at the keyframes.
    /// </summary>
    Discrete = 1,

    /// <summary>
    /// Same as <see cref="Continuous"/> but works as a flag to capture the value of the current object 
    /// and perform interpolation in some methods. 
    /// See also <see cref="Polytoria.Datamodel.AnimationMixer.Capture"/>, <see cref="Polytoria.Datamodel.AnimationPlayer.PlaybackAutoCapture"/>, 
    /// and <see cref="Polytoria.Datamodel.AnimationPlayer.PlayWithCapture"/>.
    /// </summary>
    Capture = 2
}