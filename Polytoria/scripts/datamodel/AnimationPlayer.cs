// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Godot;
using Polytoria.Attributes;
using Polytoria.Enums;
using Polytoria.Networking;
using Polytoria.Scripting;

using static Polytoria.Utils.AntiCorruption;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationPlayer is an anti-corruption layer for interfacing with Godot.AnimationPlayer.

/// <summary>
/// An animation player is used for general-purpose playback of <see cref="Animation"/>s. It contains a dictionary of <see cref="AnimationLibrary"/> resources and custom blend times between animation transitions.
/// <para>Some methods and properties use a single key to reference an animation directly. These keys are formatted as the key for the library, followed by a forward slash, then the key for the animation within the library, for example <c>"movement/run"</c>. If the library's key is an empty string (known as the default library), the forward slash is omitted, being the same key used by the library.</para>
/// <para><strong>AnimationPlayer</strong> is better-suited than <see cref="Tween"/> for more complex animations, for example ones with non-trivial timings. It can also be used over <see cref="Tween"/> if the animation track editor is more convenient than doing it in code.</para>
/// <para>Updating the target properties of animations occurs at the process frame.</para>
/// </summary>
[Instantiable]
public partial class AnimationPlayer : AnimationMixer
{
	// ---------------------------------------------- Internal Logic ---------------------------------------------------

	internal Godot.AnimationPlayer GDAnimationPlayer = null!;
	protected override Godot.AnimationMixer GDAnimationMixer => GDAnimationPlayer;

	private string _assignedAnimation = "";
	private string _autoplay = "";
	private bool _movieQuitOnFinish = false;
	private bool _playbackAutoCapture = true;
	private float _playbackAutoCaptureDuration = -1.0f;
	private EaseTypeEnum _playbackAutoCaptureEaseType = EaseTypeEnum.In;
	private TransitionTypeEnum _playbackAutoCaptureTransitionType = TransitionTypeEnum.Linear;
	private float _playbackDefaultBlendTime = 0.0f;
	private float _speedScale = 1.0f;

	// ------------------------------------------------ Properties -----------------------------------------------------

	/// <summary>
	/// If playing, the current <see cref="Animation"/>'s key, otherwise, the animation last played.
	/// When set, this changes the animation, but will not play it unless already playing.
	/// See also <see cref="CurrentAnimation"/>.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue("")]
	public string AssignedAnimation
	{
		get => _assignedAnimation;
		set
		{
			if (value != null)
			{
				if (GDAnimationPlayer.HasAnimation(value))
				{
					_assignedAnimation = value;
					GDAnimationPlayer.AssignedAnimation = value;
				}
				else if (ShouldValidate)
					throw new ArgumentException($"No animation with the key '{value}' exists in this player.",
						nameof(value));
			}
			else if (ShouldValidate)
				throw new ArgumentNullException(nameof(value), $"AssignedAnimation cannot be nil.");
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The key of the <see cref="Animation"/> to play when the scene loads.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue("")]
	public string Autoplay
	{
		get => _autoplay;
		set
		{
			if (ShouldValidate)
				ValidateName(value);

			_autoplay = value;
			GDAnimationPlayer.Autoplay = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The key of the currently playing <see cref="Animation"/>. If no animation is playing, the property's value is an empty string.
	/// Changing this value does not restart the animation. See <see cref="Play"/> for more information on playing animations.
	/// <para><strong>Note:</strong> While this property appears in the Inspector, it's not meant to be edited, and it's not saved in the scene.
	/// This property is mainly used to get the currently playing animation, and internally for animation playback tracks.</para>
	/// </summary>
	[ScriptProperty, DefaultValue("")]
	public string CurrentAnimation
	{
		get => GDAnimationPlayer.CurrentAnimation;
	}

	/// <summary>
	/// The length (in seconds) of the currently playing <see cref="Animation"/>.
	/// </summary>
	[ScriptProperty, DefaultValue(0.0f)]
	public float CurrentAnimationLength
	{
		get => (float)GDAnimationPlayer.CurrentAnimationLength;
	}

	/// <summary>
	/// The position (in seconds) of the currently playing <see cref="Animation"/>.
	/// </summary>
	[ScriptProperty, DefaultValue(0.0f)]
	public float? CurrentAnimationPosition
	{
		get => (float)GDAnimationPlayer.CurrentAnimationPosition;
	}

	/// <summary>
	/// If <c>true</c> and the engine is running in Movie Maker mode (see <c>MovieWriter</c>), exits the engine with <c>SceneTree.Quit()</c>
	/// as soon as an animation is done playing in this <c>Polytoria.Datamodel.AnimationPlayer</c>. A message is printed when the engine quits for this reason.
	/// <para><strong>Note:</strong> This obeys the same logic as the <c>AnimationMixer.AnimationFinished</c> signal, so it will not quit the engine if the animation is set to be looping.</para>
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(false)]
	public bool MovieQuitOnFinishEnabled
	{
		get => _movieQuitOnFinish;
		set
		{
			_movieQuitOnFinish = value;
			GDAnimationPlayer.MovieQuitOnFinish = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// If <c>true</c>, performs <see cref="AnimationMixer.Capture"/> before playback automatically.
	/// This means just <see cref="PlayWithCapture"/> is executed with default arguments instead of <see cref="Play"/>.
	/// <para><strong>Note:</strong> Capture interpolation is only performed if the animation contains a capture track. See also <see cref="Animation"/>.</para>
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(true)]
	public bool PlaybackAutoCapture
	{
		get => _playbackAutoCapture;
		set
		{
			_playbackAutoCapture = value;
			GDAnimationPlayer.PlaybackAutoCapture = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Duration for auto capture. If negative, the duration is set to the interval between the current position and the first key.
	/// See also <see cref="PlayWithCapture"/> and <c>AnimationMixer.Capture()</c>.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(-1.0f)]
	public float PlaybackAutoCaptureDuration
	{
		get => _playbackAutoCaptureDuration;
		set
		{
			if (ShouldValidate)
				ValidateFinite(value);

			_playbackAutoCaptureDuration = value;
			GDAnimationPlayer.PlaybackAutoCaptureDuration = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The transition type of the capture interpolation. See also <c>TransitionType</c>.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(TransitionTypeEnum.Linear)]
	public TransitionTypeEnum PlaybackAutoCaptureTransitionType
	{
		get => _playbackAutoCaptureTransitionType;
		set
		{
			if (ShouldValidate)
				ValidateEnum(value);

			_playbackAutoCaptureTransitionType = value;
			GDAnimationPlayer.PlaybackAutoCaptureTransitionType = (Godot.Tween.TransitionType)(int)value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The ease type of the capture interpolation. See also <c>EaseType</c>.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(EaseTypeEnum.In)]
	public EaseTypeEnum PlaybackAutoCaptureEaseType
	{
		get => _playbackAutoCaptureEaseType;
		set
		{
			if (ShouldValidate)
				ValidateEnum(value);

			_playbackAutoCaptureEaseType = value;
			GDAnimationPlayer.PlaybackAutoCaptureEaseType = (Godot.Tween.EaseType)(int)value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// The default time in which to blend animations. Ranges from 0 to 4096 with 0.01 precision.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(0.0f)]
	public float PlaybackDefaultBlendTime
	{
		get => _playbackDefaultBlendTime;
		set
		{
			if (ShouldValidate)
			{
				ValidateFinite(value);
				if (value < 0.0f)
					throw new ArgumentOutOfRangeException(nameof(value), value, "PlaybackDefaultBlendTime cannot be negative.");
			}

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
	[Editable, ScriptProperty, DefaultValue(1.0)]
	public float SpeedScale
	{
		get => _speedScale;
		set
		{
			if (ShouldValidate)
				ValidateFinite(value);

			_speedScale = value;
			GDAnimationPlayer.SpeedScale = value;
			OnPropertyChanged();
		}
	}

	// -------------------------------------------------- Methods ------------------------------------------------------

	/// <summary>
	/// Returns the key of the animation which is queued to play after the <paramref name="animationFrom"/> animation.
	/// </summary>
	/// <param name="animationFrom">The key of the animation to check the next animation for.</param>
	/// <returns>The key of the next animation, or an empty string if none is queued.</returns>
	[ScriptMethod]
	public string AnimationGetNext(string animationFrom)
	{
		ValidateAnimationExists(animationFrom);

		return GDAnimationPlayer.AnimationGetNext(animationFrom);
	}

	/// <summary>
	/// Triggers the <paramref name="animationTo"/> animation when the <paramref name="animationFrom"/> animation completes.
	/// </summary>
	/// <param name="animationFrom">The key of the animation to trigger from.</param>
	/// <param name="animationTo">The key of the animation to play next.</param>
	[ScriptMethod]
	public void AnimationSetNext(string animationFrom, string animationTo)
	{
		ValidateAnimationExists(animationFrom);
		ValidateAnimationExists(animationTo);

		GDAnimationPlayer.AnimationSetNext(animationFrom, animationTo);
	}

	/// <summary>
	/// Clears all queued, unplayed animations.
	/// </summary>
	[ScriptMethod]
	public void ClearQueue() => GDAnimationPlayer.ClearQueue();

	/// <summary>
	/// Returns the blend time (in seconds) between two animations, referenced by their keys.
	/// </summary>
	/// <param name="animationFrom">The key of the source animation.</param>
	/// <param name="animationTo">The key of the destination animation.</param>
	/// <returns>The blend time in seconds.</returns>
	[ScriptMethod]
	public double GetBlendTime(string animationFrom, string animationTo)
	{
		ValidateAnimationExists(animationFrom);
		ValidateAnimationExists(animationTo);

		return GDAnimationPlayer.GetBlendTime(animationFrom, animationTo);
	}

	/// <summary>
	/// Returns the actual playing speed of current animation or <c>0</c> if not playing.
	/// This speed is the <see cref="SpeedScale"/> property multiplied by <c>customSpeed</c> argument specified when calling the <see cref="Play"/> method.
	/// Returns a negative value if the current animation is playing backwards.
	/// </summary>
	/// <returns>The actual playing speed, or 0 if no animation is playing.</returns>
	[ScriptMethod]
	public float GetPlayingSpeed() => GDAnimationPlayer.GetPlayingSpeed();

	/// <summary>
	/// Returns a list of the animation keys that are currently queued to play.
	/// </summary>
	/// <returns>An array of queued animation keys.</returns>
	[ScriptMethod]
	public string[] GetQueue()
	{
		var array = GDAnimationPlayer.GetQueue();
		var result = new string[array.Count];
		for (int i = 0; i < array.Count; i++)
		{
			result[i] = (string)array[i];
		}
		return result;
	}

	/// <summary>
	/// Returns the end time of the section currently being played.
	/// </summary>
	/// <returns>The end time in seconds, or 0 if no section is active.</returns>
	[ScriptMethod]
	public double GetSectionEndTime() => GDAnimationPlayer.GetSectionEndTime();

	/// <summary>
	/// Returns the start time of the section currently being played.
	/// </summary>
	/// <returns>The start time in seconds, or 0 if no section is active.</returns>
	[ScriptMethod]
	public double GetSectionStartTime() => GDAnimationPlayer.GetSectionStartTime();

	/// <summary>
	/// Returns <c>true</c> if an animation is currently playing with a section.
	/// </summary>
	/// <returns><c>true</c> if a section is active; otherwise, <c>false</c>.</returns>
	[ScriptMethod]
	public bool HasSection() => GDAnimationPlayer.HasSection();

	/// <summary>
	/// Returns <c>true</c> if the an animation is currently active. An animation is active if it was played by calling <see cref="Play"/>
	/// and was not finished yet, or was stopped by calling <see cref="Stop"/>.
	/// This can be used to check whether an animation is currently paused or stopped.
	/// </summary>
	/// <returns><c>true</c> if an animation is active; otherwise, <c>false</c>.</returns>
	[ScriptMethod]
	public bool IsAnimationActive() => GDAnimationPlayer.IsAnimationActive();

	/// <summary>
	/// Returns <c>true</c> if an animation is currently playing (even if <see cref="SpeedScale"/> and/or <c>customSpeed</c> are <c>0</c>).
	/// </summary>
	/// <returns><c>true</c> if an animation is playing; otherwise, <c>false</c>.</returns>
	[ScriptMethod]
	public bool IsPlaying() => GDAnimationPlayer.IsPlaying();

	/// <summary>
	/// Pauses the currently playing animation. The <see cref="CurrentAnimationPosition"/> will be kept and calling <see cref="Play"/>
	/// or <see cref="PlayBackwards"/> without arguments or with the same animation name as <see cref="AssignedAnimation"/> will resume the animation.
	/// See also <see cref="Stop"/>.
	/// </summary>
	[ScriptMethod]
	public void Pause()
	{
		GDAnimationPlayer.Pause();

		if (HasAuthority)
			Rpc(nameof(NetPause));
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetPause() => GDAnimationPlayer.Pause();

	/// <summary>
	/// Plays the <see cref="Animation"/> with key <paramref name="name"/>. Custom blend times and speed can be set.
	/// The <paramref name="fromEnd"/> option only affects when switching to a new animation track, or if the same track but at the start or end.
	/// It does not affect resuming playback that was paused in the middle of an animation.
	/// If <paramref name="customSpeed"/> is negative and <paramref name="fromEnd"/> is <c>true</c>, the animation will play backwards (equivalent to <see cref="PlayBackwards"/>).
	/// The <see cref="AnimationPlayer"/> keeps track of its current or last played animation with <see cref="AssignedAnimation"/>.
	/// If this method is called with that same animation <paramref name="name"/>, or with no <paramref name="name"/> parameter,
	/// the assigned animation will resume playing if it was paused.
	/// <para><strong>Note:</strong> The animation will be updated the next time the <see cref="AnimationPlayer"/> is processed.
	/// If other variables are updated at the same time this is called, they may be updated too early. To perform the update immediately, call <see cref="AnimationMixer.Advance"/>.</para>
	/// </summary>
	/// <param name="name">The key of the animation to play. If null, plays the assigned animation.</param>
	/// <param name="customBlend">The blend time (in seconds) to use. If negative, uses the default blend time.</param>
	/// <param name="customSpeed">The speed scale for playback (default 1.0).</param>
	/// <param name="fromEnd">If true, starts from the end of the animation (default false).</param>
	[ScriptMethod]
	public void Play(string? name = null, double customBlend = -1, float customSpeed = 1, bool fromEnd = false)
	{
		ValidateAnimationPlayable(name);
		ValidateFinite(customBlend);
		ValidateFinite(customSpeed);

		GDAnimationPlayer.Play(name == null ? null : new StringName(name), customBlend, customSpeed, fromEnd);

		// Validation already happened, so reciever can skip it.
		if (HasAuthority)
			Rpc(nameof(NetPlay), name, customBlend, customSpeed, fromEnd);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetPlay(string? name, double customBlend, float customSpeed, bool fromEnd)
		=> GDAnimationPlayer.Play(name == null ? null : new StringName(name), customBlend, customSpeed, fromEnd);

	/// <summary>
	/// Plays the <see cref="Animation"/> with key <paramref name="name"/> in reverse.
	/// This method is a shorthand for <see cref="Play"/> with <c>customSpeed = -1.0</c> and <c>fromEnd = true</c>.
	/// </summary>
	/// <param name="name">The key of the animation to play in reverse. If null, plays the assigned animation.</param>
	/// <param name="customBlend">The blend time (in seconds) to use. If negative, uses the default blend time.</param>
	[ScriptMethod]
	public void PlayBackwards(string? name = null, double customBlend = -1)
	{
		ValidateAnimationPlayable(name);
		ValidateFinite(customBlend);

		GDAnimationPlayer.PlayBackwards(name == null ? null : new StringName(name), customBlend);

		if (HasAuthority)
			Rpc(nameof(NetPlayBackwards), name, customBlend);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetPlayBackwards(string? name, double customBlend)
		=> GDAnimationPlayer.PlayBackwards(name == null ? null : new StringName(name), customBlend);

	/// <summary>
	/// Plays the <see cref="Animation"/> with key <paramref name="name"/> and the section starting from <paramref name="startTime"/> and ending on <paramref name="endTime"/>.
	/// Setting <paramref name="startTime"/> to a value outside the range of the animation means the start of the animation will be used instead,
	/// and setting <paramref name="endTime"/> to a value outside the range of the animation means the end of the animation will be used instead.
	/// <paramref name="startTime"/> cannot be equal to <paramref name="endTime"/>.
	/// </summary>
	/// <param name="name">The key of the animation to play. If null, plays the assigned animation.</param>
	/// <param name="startTime">The start time of the section in seconds. If negative, uses the animation start.</param>
	/// <param name="endTime">The end time of the section in seconds. If negative, uses the animation end.</param>
	/// <param name="customBlend">The blend time (in seconds) to use. If negative, uses the default blend time.</param>
	/// <param name="customSpeed">The speed scale for playback (default 1.0).</param>
	/// <param name="fromEnd">If true, starts from the end of the section (default false).</param>
	[ScriptMethod]
	public void PlaySection(string? name = null, double startTime = -1, double endTime = -1, double customBlend = -1,
		float customSpeed = 1, bool fromEnd = false)
	{
		ValidateAnimationPlayable(name);
		ValidateFinite(startTime);
		ValidateFinite(endTime);
		ValidateFinite(customBlend);
		ValidateFinite(customSpeed);

		if (startTime >= 0 && endTime >= 0 && startTime == endTime)
			throw new ArgumentException("Section startTime cannot be equal to endTime.", nameof(endTime));
		GDAnimationPlayer.PlaySection(name == null ? null : new StringName(name), startTime, endTime, customBlend,
			customSpeed, fromEnd);

		if (HasAuthority)
			Rpc(nameof(NetPlaySection), name, startTime, endTime, customBlend, customSpeed, fromEnd);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetPlaySection(string? name, double startTime, double endTime, double customBlend, float customSpeed,
		bool fromEnd)
		=> GDAnimationPlayer.PlaySection(name == null ? null : new StringName(name), startTime, endTime, customBlend,
			customSpeed, fromEnd);

	/// <summary>
	/// Plays the <see cref="Animation"/> with key <paramref name="name"/> and the section starting from <paramref name="startTime"/> and ending on <paramref name="endTime"/> in reverse.
	/// This method is a shorthand for <see cref="PlaySection"/> with <c>customSpeed = -1.0</c> and <c>fromEnd = true</c>.
	/// </summary>
	/// <param name="name">The key of the animation to play in reverse. If null, plays the assigned animation.</param>
	/// <param name="startTime">The start time of the section in seconds. If negative, uses the animation start.</param>
	/// <param name="endTime">The end time of the section in seconds. If negative, uses the animation end.</param>
	/// <param name="customBlend">The blend time (in seconds) to use. If negative, uses the default blend time.</param>
	[ScriptMethod]
	public void PlaySectionBackwards(string? name = null, double startTime = -1, double endTime = -1,
		double customBlend = -1)
	{
		ValidateAnimationPlayable(name);
		ValidateFinite(startTime);
		ValidateFinite(endTime);
		ValidateFinite(customBlend);
		if (startTime >= 0 && endTime >= 0 && startTime == endTime)
			throw new ArgumentException("Section startTime cannot be equal to endTime.", nameof(endTime));

		GDAnimationPlayer.PlaySectionBackwards(name == null ? null : new StringName(name), startTime, endTime,
			customBlend);

		if (HasAuthority)
			Rpc(nameof(NetPlaySectionBackwards), name, startTime, endTime, customBlend);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetPlaySectionBackwards(string? name, double startTime, double endTime, double customBlend)
		=> GDAnimationPlayer.PlaySectionBackwards(name == null ? null : new StringName(name), startTime, endTime,
			customBlend);

	/// <summary>
	/// Plays the <see cref="Animation"/> with key <paramref name="name"/> and the section starting from <paramref name="startMarker"/> and ending on <paramref name="endMarker"/>.
	/// If the start marker is empty, the section starts from the beginning of the animation.
	/// If the end marker is empty, the section ends on the end of the animation.
	/// </summary>
	/// <param name="name">The key of the animation to play. If null, plays the assigned animation.</param>
	/// <param name="startMarker">The name of the start marker. If null, uses the animation start.</param>
	/// <param name="endMarker">The name of the end marker. If null, uses the animation end.</param>
	/// <param name="customBlend">The blend time (in seconds) to use. If negative, uses the default blend time.</param>
	/// <param name="customSpeed">The speed scale for playback (default 1.0).</param>
	/// <param name="fromEnd">If true, starts from the end of the section (default false).</param>
	[ScriptMethod]
	public void PlaySectionWithMarkers(string? name = null, string? startMarker = null, string? endMarker = null,
		double customBlend = -1, float customSpeed = 1, bool fromEnd = false)
	{
		ValidateMarkers(name, startMarker, endMarker);
		ValidateFinite(customBlend);
		ValidateFinite(customSpeed);

		GDAnimationPlayer.PlaySectionWithMarkers(
			name == null ? null : new StringName(name),
			startMarker == null ? null : new StringName(startMarker),
			endMarker == null ? null : new StringName(endMarker),
			customBlend, customSpeed, fromEnd);

		if (HasAuthority)
			Rpc(nameof(NetPlaySectionWithMarkers), name, startMarker, endMarker, customBlend, customSpeed, fromEnd);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetPlaySectionWithMarkers(string? name, string? startMarker, string? endMarker, double customBlend,
		float customSpeed, bool fromEnd)
		=> GDAnimationPlayer.PlaySectionWithMarkers(
			name == null ? null : new StringName(name),
			startMarker == null ? null : new StringName(startMarker),
			endMarker == null ? null : new StringName(endMarker),
			customBlend, customSpeed, fromEnd);

	/// <summary>
	/// Plays the <see cref="Animation"/> with key <paramref name="name"/> and the section starting from <paramref name="startMarker"/> and ending on <paramref name="endMarker"/> in reverse.
	/// This method is a shorthand for <see cref="PlaySectionWithMarkers"/> with <c>customSpeed = -1.0</c> and <c>fromEnd = true</c>.
	/// </summary>
	/// <param name="name">The key of the animation to play in reverse. If null, plays the assigned animation.</param>
	/// <param name="startMarker">The name of the start marker. If null, uses the animation start.</param>
	/// <param name="endMarker">The name of the end marker. If null, uses the animation end.</param>
	/// <param name="customBlend">The blend time (in seconds) to use. If negative, uses the default blend time.</param>
	[ScriptMethod]
	public void PlaySectionWithMarkersBackwards(string? name = null, string? startMarker = null,
		string? endMarker = null, double customBlend = -1)
	{
		PlaySectionWithMarkers(name, startMarker, endMarker, customBlend, -1.0f, true);
	}

	/// <summary>
	/// You can use this method to use more detailed options for capture than those performed by <see cref="PlaybackAutoCapture"/>.
	/// When <see cref="PlaybackAutoCapture"/> is <c>false</c>, this method is almost the same as calling <see cref="AnimationMixer.Capture"/> followed by <see cref="Play"/>.
	/// If <paramref name="name"/> is blank, it specifies <see cref="AssignedAnimation"/>.
	/// If <paramref name="duration"/> is a negative value, the duration is set to the interval between the current position and the first key.
	/// When <paramref name="fromEnd"/> is <c>true</c>, uses the interval between the current position and the last key instead.
	/// <para><strong>Note:</strong> The <paramref name="duration"/> takes <see cref="SpeedScale"/> into account, but <paramref name="customSpeed"/> does not.</para>
	/// </summary>
	/// <param name="name">The key of the animation to play. If null, plays the assigned animation.</param>
	/// <param name="duration">The duration for capture interpolation in seconds. If negative, uses the interval to the first/last key.</param>
	/// <param name="customBlend">The blend time (in seconds) to use. If negative, uses the default blend time.</param>
	/// <param name="customSpeed">The speed scale for playback (default 1.0).</param>
	/// <param name="fromEnd">If true, starts from the end of the animation (default false).</param>
	/// <param name="transType">The transition type for capture interpolation (default Linear).</param>
	/// <param name="easeType">The ease type for capture interpolation (default In).</param>
	[ScriptMethod]
	public void PlayWithCapture(string? name = null, double duration = -1, double customBlend = -1,
		float customSpeed = 1, bool fromEnd = false, TransitionTypeEnum transType = TransitionTypeEnum.Linear,
		EaseTypeEnum easeType = EaseTypeEnum.In)
	{
		ValidateAnimationPlayable(name);
		ValidateFinite(duration);
		ValidateFinite(customBlend);
		ValidateFinite(customSpeed);
		ValidateEnum(transType);
		ValidateEnum(easeType);

		GDAnimationPlayer.PlayWithCapture(name == null ? null : new StringName(name), duration, customBlend,
			customSpeed, fromEnd, (Godot.Tween.TransitionType)(int)transType, (Godot.Tween.EaseType)(int)easeType);
	}

	/// <summary>
	/// Queues an <see cref="Animation"/> for playback once the current animation and all previously queued animations are done.
	/// <para><strong>Note:</strong> If a looped animation is currently playing, the queued animation will never play unless the looped animation is stopped somehow.</para>
	/// </summary>
	/// <param name="name">The key of the animation to queue for playback.</param>
	[ScriptMethod]
	public void Queue(string name)
	{
		ValidateName(name);

		GDAnimationPlayer.Queue(name);
	}

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
	/// <para><strong>Note:</strong> Seeking to the end of the animation doesn't emit <see cref="AnimationMixer.AnimationFinished"/>.
	/// If you want to skip animation and emit the signal, use <see cref="AnimationMixer.Advance"/>.</para>
	/// </summary>
	/// <param name="seconds">The time in seconds to seek to.</param>
	/// <param name="update">If true, immediately updates the animation. Otherwise updates at process time (default false).</param>
	/// <param name="updateOnly">If true, does not process method, audio, or animation playback tracks (default false).</param>
	[ScriptMethod]
	public void Seek(double seconds, bool update = false, bool updateOnly = false)
	{
		ValidateFinite(seconds);

		GDAnimationPlayer.Seek(seconds, update, updateOnly);

		if (HasAuthority)
			Rpc(nameof(NetSeek), seconds, update, updateOnly);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetSeek(double seconds, bool update, bool updateOnly)
		=> GDAnimationPlayer.Seek(seconds, update, updateOnly);

	/// <summary>
	/// Specifies a blend time (in seconds) between two animations, referenced by their keys.
	/// </summary>
	/// <param name="animationFrom">The key of the source animation.</param>
	/// <param name="animationTo">The key of the destination animation.</param>
	/// <param name="sec">The blend time in seconds.</param>
	[ScriptMethod]
	public void SetBlendTime(string animationFrom, string animationTo, double sec)
	{
		ValidateName(animationFrom);
		ValidateName(animationTo);
		ValidateFinite(sec);
		if (sec < 0)
			throw new ArgumentOutOfRangeException(nameof(sec), sec, "Blend time cannot be negative.");

		GDAnimationPlayer.SetBlendTime(animationFrom, animationTo, sec);
	}

	/// <summary>
	/// Changes the start and end times of the section being played. The current playback position will be clamped within the new section.
	/// See also <see cref="PlaySection"/>.
	/// </summary>
	/// <param name="startTime">The start time of the section in seconds. Must be non-negative.</param>
	/// <param name="endTime">The end time of the section in seconds. Must be non-negative and greater than startTime.</param>
	[ScriptMethod]
	public void SetSection(double startTime = -1, double endTime = -1)
	{
		ValidateFinite(startTime);
		ValidateFinite(endTime);
		if (startTime < 0)
			throw new ArgumentException("Section startTime cannot be negative.", nameof(startTime));
		if (endTime < 0)
			throw new ArgumentException("Section endTime cannot be negative.", nameof(startTime));
		if (startTime > endTime)
			throw new ArgumentException("Section startTime cannot be greater than endTime.", nameof(startTime));
		if (startTime == endTime)
			throw new ArgumentException("Section startTime cannot be equal to endTime.", nameof(startTime));

		GDAnimationPlayer.SetSection(startTime, endTime);
	}

	/// <summary>
	/// Changes the start and end markers of the section being played. The current playback position will be clamped within the new section.
	/// See also <see cref="PlaySectionWithMarkers"/>.
	/// If the argument is empty, the section uses the beginning or end of the animation. If both are empty, it means that the section is not set.
	/// </summary>
	/// <param name="startMarker">The name of the start marker. If null, uses the animation start.</param>
	/// <param name="endMarker">The name of the end marker. If null, uses the animation end.</param>
	[ScriptMethod]
	public void SetSectionWithMarkers(string? startMarker = null, string? endMarker = null)
		=> GDAnimationPlayer.SetSectionWithMarkers(
			startMarker == null ? null : new StringName(startMarker),
			endMarker == null ? null : new StringName(endMarker));

	/// <summary>
	/// Stops the currently playing <see cref="Animation"/>. The animation position is reset to <c>0</c> and the <c>customSpeed</c> is reset to <c>1.0</c>.
	/// See also <see cref="Pause"/>.
	/// If <paramref name="keepState"/> is <c>true</c>, the animation state is not updated visually.
	/// <para><strong>Note:</strong> The method / audio / animation playback tracks will not be processed by this method.</para>
	/// </summary>
	/// <param name="keepState">If true, the animation state is not updated visually (default false).</param>
	[ScriptMethod]
	public void Stop(bool keepState = false)
	{
		GDAnimationPlayer.Stop(keepState);

		if (HasAuthority)
			Rpc(nameof(NetStop), keepState);
	}

	[NetRpc(AuthorityMode.Authority, TransferMode = TransferMode.Reliable)]
	private void NetStop(bool keepState) => GDAnimationPlayer.Stop(keepState);

	// ------------------------------------------------ Signals --------------------------------------------------------

	/// <summary>
	/// Emitted when a queued animation plays after the previous animation finished. See also <see cref="Queue"/>.
	/// <para><strong>Note:</strong> The signal is not emitted when the animation is changed via <see cref="Play"/> or by an <see cref="AnimationTree"/>.</para>
	/// </summary>
	[ScriptProperty]
	public PTSignal<string, string> AnimationChanged { get; private set; } = new();

	/// <summary>
	/// Emitted when <see cref="CurrentAnimation"/> changes.
	/// </summary>
	[ScriptProperty]
	public PTSignal<string> CurrentAnimationChanged { get; private set; } = new();

	private void OnAnimationChanged(StringName oldName, StringName newName)
	{
		AnimationChanged.Invoke((string)oldName, (string)newName);
	}

	private void OnCurrentAnimationChanged(StringName name)
	{
		CurrentAnimationChanged.Invoke((string)name);
	}

	// ---------------------------------------------- Init and Deinit --------------------------------------------------

	// TODO: Double-check all of the network replication logic.
	public override Node CreateGDNode()
	{
		return new Godot.AnimationPlayer();
	}

	public override void InitGDNode()
	{
		GDAnimationPlayer = (Godot.AnimationPlayer)GDNode;
		base.InitGDNode();
	}

	public override void Init()
	{
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

	// ----------------------------------------------- Validation ------------------------------------------------------

	// Ensures an animation with the given key exists before reading, renaming, or removing it.
	private void ValidateAnimationExists(string animation)
	{
		ValidateName(animation, nameof(animation));
		if (!GDAnimationPlayer.HasAnimation(animation))
			throw new ArgumentNullException(nameof(animation), $"Animation with key '{animation}' does not exist.");
	}

	// Ensures an animation with the given key is playable.
	private void ValidateAnimationPlayable(string? animation)
	{
		if (animation == null)
		{
			animation = GDAnimationPlayer.CurrentAnimation;
			if (animation == "")
				throw new ArgumentNullException(nameof(animation), $"No animation has been assigned to the player.");
		}
		animation ??= GDAnimationPlayer.CurrentAnimation;
		if (animation == "")
			throw new ArgumentException(
				$"An empty string cannot be used to use the default animation, use nil instead.", nameof(animation));
		if (!GDAnimationPlayer.HasAnimation(animation))
			throw new ArgumentNullException(nameof(animation), $"Animation with key '{animation}' does not exist.");
	}

	// Ensures a marker with the given key exists in the target animation.
	private void ValidateMarker(Animation animation, string? marker, string? markerName = null)
	{
		if (marker == "")
			throw new ArgumentException(
				$"An empty string cannot be used to indicate no marker, use nil instead.", nameof(marker));
		if (marker != null && !animation.HasMarker(marker))
			throw new ArgumentNullException(
				markerName, $"Marker with key '{marker}' does not exist in animation '{animation.Name}'.");
	}

	// Ensures a markers with the given key exist in the target animation, and that the target animation exists.
	private void ValidateMarkers(string? animationName, string? startMarker, string? endMarker)
	{
		ValidateAnimationPlayable(animationName);
		animationName ??= GDAnimationPlayer.CurrentAnimation;
		Animation? animation = GDAnimationPlayer.GetAnimation(animationName);
		if (animation != null) // Not null per ValidateAnimationPlayable; just vanquishing annoying warning.
		{
			ValidateMarker(animation, startMarker);
			ValidateMarker(animation, endMarker);
		}
	}
}
