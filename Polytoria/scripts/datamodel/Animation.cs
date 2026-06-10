// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using Godot;
using Polytoria.Attributes;
using Polytoria.Enums;
using Polytoria.Scripting.Datatypes;

namespace Polytoria.Datamodel;

// Polytoria.Datamodel.AnimationMixer is an anti-corruption layer for interfacing with Godot.AnimationMixer.

/// <summary>
/// Base class for <see cref="AnimationPlayer"/> and <see cref="AnimationTree"/> to manage animation lists. It also has general properties and methods for playback and blending.
/// <para>After instantiating the playback information data within the extended class, the blending is processed by the <c>AnimationMixer</c>.</para>
/// </summary>
[Instantiable]
public partial class Animation : Instance
{
    private static readonly ConditionalWeakTable<Godot.Animation, Animation> GDAnimations = [];
    private Godot.Animation GDAnimation = null!;
    private readonly bool _captureIncluded = false;
    private float _length = 1.0f;
    private LoopModeEnum _loopMode = LoopModeEnum.None;
    private float _step = 1f/60f;

    /// <summary>
    /// Returns <c>true</c> if the capture track is included. This is a cached readonly value for performance.
    /// </summary>
	[Editable, ScriptProperty, DefaultValue(false)]
    public bool CaptureIncluded { get => _captureIncluded; }

    /// <summary>
    /// The total length of the animation (in seconds).
    /// <para><strong>Note:</strong> Length is not delimited by the last key, as this one may be before or after the end to ensure correct interpolation and looping.</para>
    /// </summary>
	[Editable, ScriptProperty, DefaultValue(1)]
    public float Length {
        get => _length;
        set
        {
            _length = value;
            GDAnimation.Length = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Determines the behavior of both ends of the animation timeline during animation playback. This indicates whether and how the animation should be restarted, and is also used to correctly interpolate animation cycles.
    /// </summary>
	[Editable, ScriptProperty, DefaultValue(LoopModeEnum.None)]
    public LoopModeEnum LoopMode {
        get => _loopMode;
        set
        {
            _loopMode = value;
            GDAnimation.LoopMode = (Godot.Animation.LoopModeEnum)value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The animation step value.
    /// </summary>
	[Editable, ScriptProperty, DefaultValue(1f/60f)]
    public float Step {
        get => _step;
        set
        {
            float val = Math.Max(1f/120f, value);
            _step = val;
            GDAnimation.Step = val;
            OnPropertyChanged();
        }
    }

    // Intialize an Animation from a Godot type, this is done to mitigate possible memory leaks.
    private static Animation FromGDObject(Godot.Animation gdAnimation)
    {
        return Polytoria.Shared.Globals.LoadInstance<Animation>(World.Current, anim => anim.GDAnimation = gdAnimation);
    }

    // Implicit conversion from ACL type to Godot type.
    public static implicit operator Godot.Animation(Animation acl) => acl.GDAnimation;

	// Implicit conversion from Godot type to ACL type.
    public static implicit operator Animation(Godot.Animation gd) => GDAnimations.GetOrAdd(gd, _ => FromGDObject(gd));

	/// <summary>
	/// Adds a marker to this Animation.
	/// </summary>
	/// <param name="name">The name of the marker.</param>
	/// <param name="time">The time (in seconds) where the marker is placed.</param>
	[ScriptMethod]
    public void AddMarker(PTStringName name, float time)
    {
        GDAnimation.AddMarker(name, time);
    }

    /// <summary>
    /// Adds a track to the Animation.
    /// </summary>
    /// <param name="type">The type of the track to add.</param>
    /// <param name="atPosition">The position in the track list where the new track should be inserted. Use -1 to append at the end.</param>
    /// <returns>The index of the newly created track.</returns>
    [ScriptMethod]
    public int AddTrack(TrackTypeEnum type, int atPosition = -1)
    {
        return GDAnimation.AddTrack((Godot.Animation.TrackType)type, atPosition);
    }

    /// <summary>
    /// Returns the animation name at the key identified by <paramref name="keyIdx"/>. The <paramref name="trackIdx"/> must be the index of an Animation Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Animation Track.</param>
    /// <param name="keyIdx">The index of the key within the track.</param>
    /// <returns>The animation name (StringName) at the specified key.</returns>
    [ScriptMethod]
    public PTStringName AnimationTrackGetKeyAnimation(int trackIdx, int keyIdx)
    {
        return GDAnimation.AnimationTrackGetKeyAnimation(trackIdx, keyIdx);
    }

    /// <summary>
    /// Inserts a key with value <paramref name="animation"/> at the given <paramref name="time"/> (in seconds). The <paramref name="trackIdx"/> must be the index of an Animation Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Animation Track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="animation">The animation name to set for this key.</param>
    /// <returns>The index of the newly inserted key.</returns>
    [ScriptMethod]
    public int AnimationTrackInsertKey(int trackIdx, float time, PTStringName animation)
    {
        return GDAnimation.AnimationTrackInsertKey(trackIdx, time, animation);
    }

    /// <summary>
    /// Sets the key identified by <paramref name="keyIdx"/> to value <paramref name="animation"/>. The <paramref name="trackIdx"/> must be the index of an Animation Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Animation Track.</param>
    /// <param name="keyIdx">The index of the key to set.</param>
    /// <param name="animation">The animation name to assign to the key.</param>
    [ScriptMethod]
    public void AnimationTrackSetKeyAnimation(int trackIdx, int keyIdx, PTStringName animation)
    {
        GDAnimation.AnimationTrackSetKeyAnimation(trackIdx, keyIdx, animation);
    }

    /// <summary>
    /// Returns the end offset of the key identified by <paramref name="keyIdx"/>. The <paramref name="trackIdx"/> must be the index of an Audio Track.
    /// <para>End offset is the number of seconds cut off at the ending of the audio stream.</para>
    /// </summary>
    /// <param name="trackIdx">The index of the Audio Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The end offset in seconds.</returns>
    [ScriptMethod]
    public float AudioTrackGetKeyEndOffset(int trackIdx, int keyIdx)
    {
        return GDAnimation.AudioTrackGetKeyEndOffset(trackIdx, keyIdx);
    }

    /// <summary>
    /// Returns the start offset of the key identified by <paramref name="keyIdx"/>. The <paramref name="trackIdx"/> must be the index of an Audio Track.
    /// <para>Start offset is the number of seconds cut off at the beginning of the audio stream.</para>
    /// </summary>
    /// <param name="trackIdx">The index of the Audio Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The start offset in seconds.</returns>
/*     [ScriptMethod]
    public float AudioTrackGetKeyStartOffset(int trackIdx, int keyIdx)
    {
        return GDAnimation.AudioTrackGetKeyStartOffset(trackIdx, keyIdx);
    } */

    /// <summary>
    /// Returns the audio stream of the key identified by <paramref name="keyIdx"/>. The <paramref name="trackIdx"/> must be the index of an Audio Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Audio Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The AudioStream resource assigned to the key.</returns>
/*     [ScriptMethod]
    public Resource AudioTrackGetKeyStream(int trackIdx, int keyIdx)
    {
        return GDAnimation.AudioTrackGetKeyStream(trackIdx, keyIdx);
    } */

    /// <summary>
    /// Inserts an Audio Track key at the given <paramref name="time"/> in seconds. The <paramref name="trackIdx"/> must be the index of an Audio Track.
    /// <para><paramref name="stream"/> is the AudioStream resource to play. <paramref name="startOffset"/> is the number of seconds cut off at the beginning of the audio stream, while <paramref name="endOffset"/> is at the ending.</para>
    /// </summary>
    /// <param name="trackIdx">The index of the Audio Track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="stream">The AudioStream resource to play.</param>
    /// <param name="startOffset">The number of seconds to skip at the start of the audio stream.</param>
    /// <param name="endOffset">The number of seconds to cut from the end of the audio stream.</param>
    /// <returns>The index of the newly inserted key.</returns>
/*     [ScriptMethod]
    public int AudioTrackInsertKey(int trackIdx, float time, Resource stream, float startOffset = 0f, float endOffset = 0f)
    {
        return GDAnimation.AudioTrackInsertKey(trackIdx, time, stream, startOffset, endOffset);
    } */

    /// <summary>
    /// Returns <c>true</c> if the track at <paramref name="trackIdx"/> will be blended with other animations.
    /// </summary>
    /// <param name="trackIdx">The index of the track to check.</param>
    /// <returns><c>true</c> if blending is enabled for this track; otherwise, <c>false</c>.</returns>
    [ScriptMethod]
    public bool AudioTrackIsUseBlend(int trackIdx)
    {
        return GDAnimation.AudioTrackIsUseBlend(trackIdx);
    }

    /// <summary>
    /// Sets the end offset of the key identified by <paramref name="keyIdx"/> to value <paramref name="offset"/>. The <paramref name="trackIdx"/> must be the index of an Audio Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Audio Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="offset">The new end offset in seconds.</param>
    [ScriptMethod]
    public void AudioTrackSetKeyEndOffset(int trackIdx, int keyIdx, float offset)
    {
        GDAnimation.AudioTrackSetKeyEndOffset(trackIdx, keyIdx, offset);
    }

    /// <summary>
    /// Sets the start offset of the key identified by <paramref name="keyIdx"/> to value <paramref name="offset"/>. The <paramref name="trackIdx"/> must be the index of an Audio Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Audio Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="offset">The new start offset in seconds.</param>
    [ScriptMethod]
    public void AudioTrackSetKeyStartOffset(int trackIdx, int keyIdx, float offset)
    {
        GDAnimation.AudioTrackSetKeyStartOffset(trackIdx, keyIdx, offset);
    }

    /// <summary>
    /// Sets the stream of the key identified by <paramref name="keyIdx"/> to value <paramref name="stream"/>. The <paramref name="trackIdx"/> must be the index of an Audio Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Audio Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="stream">The new AudioStream resource.</param>
    [ScriptMethod]
    public void AudioTrackSetKeyStream(int trackIdx, int keyIdx, Resource stream)
    {
        GDAnimation.AudioTrackSetKeyStream(trackIdx, keyIdx, stream);
    }

    /// <summary>
    /// Sets whether the track will be blended with other animations. If <c>true</c>, the audio playback volume changes depending on the blend value.
    /// </summary>
    /// <param name="trackIdx">The index of the track to modify.</param>
    /// <param name="enable">Whether to enable blending for this track.</param>
/*     [ScriptMethod]
    public void AudioTrackSetUseBlend(int trackIdx, bool enable)
    {
        GDAnimation.AudioTrackSetUseBlend(trackIdx, enable);
    } */

    /// <summary>
    /// Returns the in handle of the key identified by <paramref name="keyIdx"/>. The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The in handle vector for the Bezier curve.</returns>
    [ScriptMethod]
    public Vector2 BezierTrackGetKeyInHandle(int trackIdx, int keyIdx)
    {
        return GDAnimation.BezierTrackGetKeyInHandle(trackIdx, keyIdx);
    }

    /// <summary>
    /// Returns the out handle of the key identified by <paramref name="keyIdx"/>. The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The out handle vector for the Bezier curve.</returns>
    [ScriptMethod]
    public Vector2 BezierTrackGetKeyOutHandle(int trackIdx, int keyIdx)
    {
        return GDAnimation.BezierTrackGetKeyOutHandle(trackIdx, keyIdx);
    }

    /// <summary>
    /// Returns the value of the key identified by <paramref name="keyIdx"/>. The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The float value at the key.</returns>
    [ScriptMethod]
    public float BezierTrackGetKeyValue(int trackIdx, int keyIdx)
    {
        return GDAnimation.BezierTrackGetKeyValue(trackIdx, keyIdx);
    }

    /// <summary>
    /// Inserts a Bezier Track key at the given <paramref name="time"/> in seconds. The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// <para><paramref name="inHandle"/> is the left-side weight of the added Bezier curve point, <paramref name="outHandle"/> is the right-side one, while <paramref name="value"/> is the actual value at this point.</para>
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="value">The value at the key point.</param>
    /// <param name="inHandle">The in handle vector for the Bezier curve.</param>
    /// <param name="outHandle">The out handle vector for the Bezier curve.</param>
    /// <returns>The index of the newly inserted key.</returns>
    [ScriptMethod]
    public int BezierTrackInsertKey(int trackIdx, float time, float value, Vector2 inHandle = default, Vector2 outHandle = default)
    {
        return GDAnimation.BezierTrackInsertKey(trackIdx, time, value, inHandle, outHandle);
    }

    /// <summary>
    /// Returns the interpolated value at the given <paramref name="time"/> (in seconds). The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="time">The time (in seconds) to interpolate.</param>
    /// <returns>The interpolated float value.</returns>
    [ScriptMethod]
    public float BezierTrackInterpolate(int trackIdx, float time)
    {
        return GDAnimation.BezierTrackInterpolate(trackIdx, time);
    }

    /// <summary>
    /// Sets the in handle of the key identified by <paramref name="keyIdx"/> to value <paramref name="inHandle"/>. The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="inHandle">The new in handle vector.</param>
    /// <param name="balancedValueTimeRatio">A ratio affecting the balance between value and time (default 1.0).</param>
    [ScriptMethod]
    public void BezierTrackSetKeyInHandle(int trackIdx, int keyIdx, Vector2 inHandle, float balancedValueTimeRatio = 1.0f)
    {
        GDAnimation.BezierTrackSetKeyInHandle(trackIdx, keyIdx, inHandle, balancedValueTimeRatio);
    }

    /// <summary>
    /// Sets the out handle of the key identified by <paramref name="keyIdx"/> to value <paramref name="outHandle"/>. The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="outHandle">The new out handle vector.</param>
    /// <param name="balancedValueTimeRatio">A ratio affecting the balance between value and time (default 1.0).</param>
    [ScriptMethod]
    public void BezierTrackSetKeyOutHandle(int trackIdx, int keyIdx, Vector2 outHandle, float balancedValueTimeRatio = 1.0f)
    {
        GDAnimation.BezierTrackSetKeyOutHandle(trackIdx, keyIdx, outHandle, balancedValueTimeRatio);
    }

    /// <summary>
    /// Sets the value of the key identified by <paramref name="keyIdx"/> to the given <paramref name="value"/>. The <paramref name="trackIdx"/> must be the index of a Bezier Track.
    /// </summary>
    /// <param name="trackIdx">The index of the Bezier Track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="value">The new float value.</param>
    [ScriptMethod]
    public void BezierTrackSetKeyValue(int trackIdx, int keyIdx, float value)
    {
        GDAnimation.BezierTrackSetKeyValue(trackIdx, keyIdx, value);
    }

    /// <summary>
    /// Inserts a key in a given blend shape track. Returns the key index.
    /// </summary>
    /// <param name="trackIdx">The index of the blend shape track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="amount">The blend shape amount (0.0 to 1.0 typically).</param>
    /// <returns>The index of the newly inserted key.</returns>
    [ScriptMethod]
    public int BlendShapeTrackInsertKey(int trackIdx, float time, float amount)
    {
        return GDAnimation.BlendShapeTrackInsertKey(trackIdx, time, amount);
    }

    /// <summary>
    /// Returns the interpolated blend shape value at the given time (in seconds). The <paramref name="trackIdx"/> must be the index of a blend shape track.
    /// </summary>
    /// <param name="trackIdx">The index of the blend shape track.</param>
    /// <param name="timeSec">The time (in seconds) to interpolate.</param>
    /// <param name="backward">If true, searches backwards for the previous key if no exact match is found.</param>
    /// <returns>The interpolated blend shape value.</returns>
    [ScriptMethod]
    public float BlendShapeTrackInterpolate(int trackIdx, float timeSec, bool backward = false)
    {
        return GDAnimation.BlendShapeTrackInterpolate(trackIdx, timeSec, backward);
    }

    /// <summary>
    /// Clear the animation (clear all tracks and reset all).
    /// </summary>
    [ScriptMethod]
    public void Clear()
    {
        GDAnimation.Clear();
    }

    /// <summary>
    /// Compress the animation and all its tracks in-place. This will make <see cref="TrackIsCompressed"/> return <c>true</c> once called on this <strong>Animation</strong>. Compressed tracks require less memory to be played, and are designed to be used for complex 3D animations (such as cutscenes) imported from external 3D software. Compression is lossy, but the difference is usually not noticeable in real world conditions.
    /// <para><strong>Note:</strong> Compressed tracks have various limitations (such as not being editable from the editor), so only use compressed animations if you actually need them.</para>
    /// </summary>
    /// <param name="pageSize">The page size for compression (default 8192).</param>
    /// <param name="fps">The target frames per second for compression (default 120).</param>
    /// <param name="splitTolerance">The tolerance for splitting keys (default 4.0).</param>
    [ScriptMethod]
    public void Compress(uint pageSize = 8192, uint fps = 120, float splitTolerance = 4.0f)
    {
        GDAnimation.Compress(pageSize, fps, splitTolerance);
    }

    /// <summary>
    /// Adds a new track to <paramref name="toAnimation"/> that is a copy of the given track from this animation.
    /// </summary>
    /// <param name="trackIdx">The index of the track to copy from this animation.</param>
    /// <param name="toAnimation">The target animation to copy the track into.</param>
    [ScriptMethod]
    public void CopyTrack(int trackIdx, Animation toAnimation)
    {
        GDAnimation.CopyTrack(trackIdx, toAnimation.GDAnimation);
    }

    /// <summary>
    /// Returns the index of the specified track. If the track is not found, return -1.
    /// </summary>
    /// <param name="path">The NodePath of the property/bone being animated.</param>
    /// <param name="type">The type of track to search for.</param>
    /// <returns>The index of the track, or -1 if not found.</returns>
    [ScriptMethod]
    public int FindTrack(NodePath path, TrackTypeEnum type)
    {
        return GDAnimation.FindTrack(path, (Godot.Animation.TrackType)type);
    }

    /// <summary>
    /// Returns the name of the marker located at the given time.
    /// </summary>
    /// <param name="time">The time (in seconds) to check for a marker.</param>
    /// <returns>The name of the marker, or an empty StringName if none exists.</returns>
    [ScriptMethod]
    public PTStringName GetMarkerAtTime(float time)
    {
        return GDAnimation.GetMarkerAtTime(time);
    }

    /// <summary>
    /// Returns the given marker's color.
    /// </summary>
    /// <param name="name">The name of the marker.</param>
    /// <returns>The Color of the marker.</returns>
    [ScriptMethod]
    public Color GetMarkerColor(PTStringName name)
    {
        return GDAnimation.GetMarkerColor(name);
    }

    /// <summary>
    /// Returns every marker in this Animation, sorted ascending by time.
    /// </summary>
    /// <returns>An array of marker names.</returns>
    [ScriptMethod]
    public string[] GetMarkerNames()
    {
        return GDAnimation.GetMarkerNames();
    }

    /// <summary>
    /// Returns the given marker's time.
    /// </summary>
    /// <param name="name">The name of the marker.</param>
    /// <returns>The time (in seconds) of the marker.</returns>
    [ScriptMethod]
    public double GetMarkerTime(PTStringName name)
    {
        return GDAnimation.GetMarkerTime(name);
    }

    /// <summary>
    /// Returns the closest marker that comes after the given time. If no such marker exists, an empty string is returned.
    /// </summary>
    /// <param name="time">The reference time (in seconds).</param>
    /// <returns>The name of the next marker, or an empty StringName.</returns>
    [ScriptMethod]
    public PTStringName GetNextMarker(float time)
    {
        return GDAnimation.GetNextMarker(time);
    }

    /// <summary>
    /// Returns the closest marker that comes before the given time. If no such marker exists, an empty string is returned.
    /// </summary>
    /// <param name="time">The reference time (in seconds).</param>
    /// <returns>The name of the previous marker, or an empty StringName.</returns>
    [ScriptMethod]
    public PTStringName GetPrevMarker(float time)
    {
        return GDAnimation.GetPrevMarker(time);
    }

    /// <summary>
    /// Returns the amount of tracks in the animation.
    /// </summary>
    /// <returns>The number of tracks.</returns>
    [ScriptMethod]
    public int GetTrackCount()
    {
        return GDAnimation.GetTrackCount();
    }

    /// <summary>
    /// Returns <c>true</c> if this Animation contains a marker with the given name.
    /// </summary>
    /// <param name="name">The name of the marker to check.</param>
    /// <returns><c>true</c> if the marker exists; otherwise, <c>false</c>.</returns>
    [ScriptMethod]
    public bool HasMarker(PTStringName name)
    {
        return GDAnimation.HasMarker(name);
    }

    // Need to be very careful with this.
    /// <summary>
    /// Returns the method name of a method track.
    /// </summary>
    /// <param name="trackIdx">The index of the method track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The name of the method to call.</returns>
/*     [ScriptMethod]
    public PTStringName MethodTrackGetName(int trackIdx, int keyIdx)
    {
        return GDAnimation.MethodTrackGetName(trackIdx, keyIdx);
    } */

    // Need to be very careful with this.
    /// <summary>
    /// Returns the arguments values to be called on a method track for a given key in a given track.
    /// </summary>
    /// <param name="trackIdx">The index of the method track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>An Array containing the parameter values.</returns>
/*     [ScriptMethod]
    public Array MethodTrackGetParams(int trackIdx, int keyIdx)
    {
        return GDAnimation.MethodTrackGetParams(trackIdx, keyIdx);
    } */

    /// <summary>
    /// Optimize the animation and all its tracks in-place. This will preserve only as many keys as are necessary to keep the animation within the specified bounds.
    /// </summary>
    /// <param name="allowedVelocityErr">The allowed velocity error (default 0.01).</param>
    /// <param name="allowedAngularErr">The allowed angular error (default 0.01).</param>
    /// <param name="precision">The precision level (default 3).</param>
    [ScriptMethod]
    public void Optimize(float allowedVelocityErr = 0.01f, float allowedAngularErr = 0.01f, int precision = 3)
    {
        GDAnimation.Optimize(allowedVelocityErr, allowedAngularErr, precision);
    }

    /// <summary>
    /// Inserts a key in a given 3D position track. Returns the key index.
    /// </summary>
    /// <param name="trackIdx">The index of the 3D position track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="position">The 3D position value.</param>
    /// <returns>The index of the newly inserted key.</returns>
    [ScriptMethod]
    public int PositionTrackInsertKey(int trackIdx, float time, Vector3 position)
    {
        return GDAnimation.PositionTrackInsertKey(trackIdx, time, position);
    }

    /// <summary>
    /// Returns the interpolated position value at the given time (in seconds). The <paramref name="trackIdx"/> must be the index of a 3D position track.
    /// </summary>
    /// <param name="trackIdx">The index of the 3D position track.</param>
    /// <param name="timeSec">The time (in seconds) to interpolate.</param>
    /// <param name="backward">If true, searches backwards for the previous key if no exact match is found.</param>
    /// <returns>The interpolated 3D position.</returns>
    [ScriptMethod]
    public Vector3 PositionTrackInterpolate(int trackIdx, float timeSec, bool backward = false)
    {
        return GDAnimation.PositionTrackInterpolate(trackIdx, timeSec, backward);
    }

    /// <summary>
    /// Removes the marker with the given name from this Animation.
    /// </summary>
    /// <param name="name">The name of the marker to remove.</param>
    [ScriptMethod]
    public void RemoveMarker(PTStringName name)
    {
        GDAnimation.RemoveMarker(name);
    }

    /// <summary>
    /// Removes a track by specifying the track index.
    /// </summary>
    /// <param name="trackIdx">The index of the track to remove.</param>
    [ScriptMethod]
    public void RemoveTrack(int trackIdx)
    {
        GDAnimation.RemoveTrack(trackIdx);
    }

    /// <summary>
    /// Inserts a key in a given 3D rotation track. Returns the key index.
    /// </summary>
    /// <param name="trackIdx">The index of the 3D rotation track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="rotation">The 3D rotation quaternion.</param>
    /// <returns>The index of the newly inserted key.</returns>
    [ScriptMethod]
    public int RotationTrackInsertKey(int trackIdx, float time, Quaternion rotation)
    {
        return GDAnimation.RotationTrackInsertKey(trackIdx, time, rotation);
    }

    /// <summary>
    /// Returns the interpolated rotation value at the given time (in seconds). The <paramref name="trackIdx"/> must be the index of a 3D rotation track.
    /// </summary>
    /// <param name="trackIdx">The index of the 3D rotation track.</param>
    /// <param name="timeSec">The time (in seconds) to interpolate.</param>
    /// <param name="backward">If true, searches backwards for the previous key if no exact match is found.</param>
    /// <returns>The interpolated 3D rotation quaternion.</returns>
    [ScriptMethod]
    public Quaternion RotationTrackInterpolate(int trackIdx, float timeSec, bool backward = false)
    {
        return GDAnimation.RotationTrackInterpolate(trackIdx, timeSec, backward);
    }

    /// <summary>
    /// Inserts a key in a given 3D scale track. Returns the key index.
    /// </summary>
    /// <param name="trackIdx">The index of the 3D scale track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="scale">The 3D scale vector.</param>
    /// <returns>The index of the newly inserted key.</returns>
    [ScriptMethod]
    public int ScaleTrackInsertKey(int trackIdx, float time, Vector3 scale)
    {
        return GDAnimation.ScaleTrackInsertKey(trackIdx, time, scale);
    }

    /// <summary>
    /// Returns the interpolated scale value at the given time (in seconds). The <paramref name="trackIdx"/> must be the index of a 3D scale track.
    /// </summary>
    /// <param name="trackIdx">The index of the 3D scale track.</param>
    /// <param name="timeSec">The time (in seconds) to interpolate.</param>
    /// <param name="backward">If true, searches backwards for the previous key if no exact match is found.</param>
    /// <returns>The interpolated 3D scale vector.</returns>
    [ScriptMethod]
    public Vector3 ScaleTrackInterpolate(int trackIdx, float timeSec, bool backward = false)
    {
        return GDAnimation.ScaleTrackInterpolate(trackIdx, timeSec, backward);
    }

    /// <summary>
    /// Sets the given marker's color.
    /// </summary>
    /// <param name="name">The name of the marker.</param>
    /// <param name="color">The new color for the marker.</param>
    [ScriptMethod]
    public void SetMarkerColor(PTStringName name, Color color)
    {
        GDAnimation.SetMarkerColor(name, color);
    }

    /// <summary>
    /// Finds the key index by time in a given track. Optionally, only find it if the approx/exact time is given.
    /// <para>If <paramref name="limit"/> is <c>true</c>, it does not return keys outside the animation range.</para>
    /// <para>If <paramref name="backward"/> is <c>true</c>, the direction is reversed in methods that rely on one directional processing.</para>
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="time">The time to search for.</param>
    /// <param name="findMode">The mode for finding the key (e.g., nearest, exact).</param>
    /// <param name="limit">If true, restricts search to within the animation range.</param>
    /// <param name="backward">If true, reverses the search direction.</param>
    /// <returns>The index of the found key, or -1 if not found.</returns>
    [ScriptMethod]
    public int TrackFindKey(int trackIdx, float time, FindModeEnum findMode = FindModeEnum.Nearest, bool limit = false, bool backward = false)
    {
        return GDAnimation.TrackFindKey(trackIdx, time, (Godot.Animation.FindMode)findMode, limit, backward);
    }

    /// <summary>
    /// Returns <c>true</c> if the track at <paramref name="trackIdx"/> wraps the interpolation loop. New tracks wrap the interpolation loop by default.
    /// </summary>
    /// <param name="trackIdx">The index of the track to check.</param>
    /// <returns><c>true</c> if the track wraps the interpolation loop; otherwise, <c>false</c>.</returns>
    [ScriptMethod]
    public bool TrackGetInterpolationLoopWrap(int trackIdx)
    {
        return GDAnimation.TrackGetInterpolationLoopWrap(trackIdx);
    }

    /// <summary>
    /// Returns the interpolation type of a given track.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <returns>The interpolation type enum value.</returns>
    [ScriptMethod]
    public InterpolationTypeEnum TrackGetInterpolationType(int trackIdx)
    {
        return (InterpolationTypeEnum)GDAnimation.TrackGetInterpolationType(trackIdx);
    }

    /// <summary>
    /// Returns the number of keys in a given track.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <returns>The count of keys in the track.</returns>
    [ScriptMethod]
    public int TrackGetKeyCount(int trackIdx)
    {
        return GDAnimation.TrackGetKeyCount(trackIdx);
    }

    /// <summary>
    /// Returns the time at which the key is located.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The time (in seconds) of the key.</returns>
    [ScriptMethod]
    public double TrackGetKeyTime(int trackIdx, int keyIdx)
    {
        return GDAnimation.TrackGetKeyTime(trackIdx, keyIdx);
    }

    /// <summary>
    /// Returns the transition curve (easing) for a specific key (see the built-in math function <see cref="Math.Ease"/>).
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The transition curve value.</returns>
    [ScriptMethod]
    public float TrackGetKeyTransition(int trackIdx, int keyIdx)
    {
        return GDAnimation.TrackGetKeyTransition(trackIdx, keyIdx);
    }

    /// <summary>
    /// Returns the value of a given key in a given track.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <returns>The value stored at the key (as a Variant).</returns>
    [ScriptMethod]
    public Variant TrackGetKeyValue(int trackIdx, int keyIdx)
    {
        return GDAnimation.TrackGetKeyValue(trackIdx, keyIdx);
    }

    /// <summary>
    /// Gets the path of a track. For more information on the path format, see <see cref="TrackSetPath"/>.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <returns>The NodePath of the track.</returns>
    [ScriptMethod]
    public NodePath TrackGetPath(int trackIdx)
    {
        return GDAnimation.TrackGetPath(trackIdx);
    }

    /// <summary>
    /// Gets the type of a track.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <returns>The track type enum value.</returns>
    [ScriptMethod]
    public TrackTypeEnum TrackGetType(int trackIdx)
    {
        return (TrackTypeEnum)GDAnimation.TrackGetType(trackIdx);
    }

    /// <summary>
    /// Inserts a generic key in a given track. Returns the key index.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="time">The time (in seconds) where the key is inserted.</param>
    /// <param name="key">The value to insert.</param>
    /// <param name="transition">The transition curve value (default 1.0).</param>
    /// <returns>The index of the newly inserted key.</returns>
    [ScriptMethod]
    public int TrackInsertKey(int trackIdx, float time, Variant key, float transition = 1.0f)
    {
        return GDAnimation.TrackInsertKey(trackIdx, time, key, transition);
    }

    /// <summary>
    /// Returns <c>true</c> if the track is compressed, <c>false</c> otherwise. See also <see cref="Compress"/>.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <returns><c>true</c> if the track is compressed; otherwise, <c>false</c>.</returns>
    [ScriptMethod]
    public bool TrackIsCompressed(int trackIdx)
    {
        return GDAnimation.TrackIsCompressed(trackIdx);
    }

    /// <summary>
    /// Returns <c>true</c> if the track at index <paramref name="trackIdx"/> is enabled.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <returns><c>true</c> if the track is enabled; otherwise, <c>false</c>.</returns>
    [ScriptMethod]
    public bool TrackIsEnabled(int trackIdx)
    {
        return GDAnimation.TrackIsEnabled(trackIdx);
    }

    /// <summary>
    /// Returns <c>true</c> if the given track is imported. Else, return <c>false</c>.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <returns><c>true</c> if the track is imported; otherwise, <c>false</c>.</returns>
    [ScriptMethod]
    public bool TrackIsImported(int trackIdx)
    {
        return GDAnimation.TrackIsImported(trackIdx);
    }

    /// <summary>
    /// Moves a track down in the track list.
    /// </summary>
    /// <param name="trackIdx">The index of the track to move.</param>
    [ScriptMethod]
    public void TrackMoveDown(int trackIdx)
    {
        GDAnimation.TrackMoveDown(trackIdx);
    }

    /// <summary>
    /// Changes the index position of track <paramref name="trackIdx"/> to the one defined in <paramref name="toIdx"/>.
    /// </summary>
    /// <param name="trackIdx">The index of the track to move.</param>
    /// <param name="toIdx">The new index position.</param>
    [ScriptMethod]
    public void TrackMoveTo(int trackIdx, int toIdx)
    {
        GDAnimation.TrackMoveTo(trackIdx, toIdx);
    }

    /// <summary>
    /// Moves a track up in the track list.
    /// </summary>
    /// <param name="trackIdx">The index of the track to move.</param>
    [ScriptMethod]
    public void TrackMoveUp(int trackIdx)
    {
        GDAnimation.TrackMoveUp(trackIdx);
    }

    /// <summary>
    /// Removes a key by index in a given track.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="keyIdx">The index of the key to remove.</param>
    [ScriptMethod]
    public void TrackRemoveKey(int trackIdx, int keyIdx)
    {
        GDAnimation.TrackRemoveKey(trackIdx, keyIdx);
    }

    /// <summary>
    /// Removes a key at <paramref name="time"/> in a given track.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="time">The time of the key to remove.</param>
    [ScriptMethod]
    public void TrackRemoveKeyAtTime(int trackIdx, float time)
    {
        GDAnimation.TrackRemoveKeyAtTime(trackIdx, time);
    }

    /// <summary>
    /// Enables/disables the given track. Tracks are enabled by default.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="enabled">Whether to enable the track.</param>
    [ScriptMethod]
    public void TrackSetEnabled(int trackIdx, bool enabled)
    {
        GDAnimation.TrackSetEnabled(trackIdx, enabled);
    }

    /// <summary>
    /// Sets the given track as imported or not.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="imported">Whether the track is imported.</param>
    [ScriptMethod]
    public void TrackSetImported(int trackIdx, bool imported)
    {
        GDAnimation.TrackSetImported(trackIdx, imported);
    }

    /// <summary>
    /// If <c>true</c>, the track at <paramref name="trackIdx"/> wraps the interpolation loop.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="interpolation">Whether to enable loop wrapping.</param>
    [ScriptMethod]
    public void TrackSetInterpolationLoopWrap(int trackIdx, bool interpolation)
    {
        GDAnimation.TrackSetInterpolationLoopWrap(trackIdx, interpolation);
    }

    /// <summary>
    /// Sets the interpolation type of a given track.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="interpolation">The interpolation type to set.</param>
    [ScriptMethod]
    public void TrackSetInterpolationType(int trackIdx, InterpolationTypeEnum interpolation)
    {
        GDAnimation.TrackSetInterpolationType(trackIdx, (Godot.Animation.InterpolationType)interpolation);
    }

    /// <summary>
    /// Sets the time of an existing key.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="time">The new time (in seconds) for the key.</param>
    [ScriptMethod]
    public void TrackSetKeyTime(int trackIdx, int keyIdx, float time)
    {
        GDAnimation.TrackSetKeyTime(trackIdx, keyIdx, time);
    }

    /// <summary>
    /// Sets the transition curve (easing) for a specific key (see the built-in math function <see cref="Math.Ease"/>).
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="transition">The new transition curve value.</param>
    [ScriptMethod]
    public void TrackSetKeyTransition(int trackIdx, int keyIdx, float transition)
    {
        GDAnimation.TrackSetKeyTransition(trackIdx, keyIdx, transition);
    }

    /// <summary>
    /// Sets the value of an existing key.
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="keyIdx">The index of the key.</param>
    /// <param name="value">The new value to set.</param>
    [ScriptMethod]
    public void TrackSetKeyValue(int trackIdx, int keyIdx, Variant value)
    {
        GDAnimation.TrackSetKeyValue(trackIdx, keyIdx, value);
    }

    /// <summary>
    /// Sets the path of a track. Paths must be valid scene-tree paths to a node and must be specified starting from the <see cref="AnimationMixer.RootNode"/> that will reproduce the animation. Tracks that control properties or bones must append their name after the path, separated by <c>":"</c>.
    /// <para>For example, <c>"character/skeleton:ankle"</c> or <c>"character/mesh:transform/local"</c>.</para>
    /// </summary>
    /// <param name="trackIdx">The index of the track.</param>
    /// <param name="path">The new NodePath for the track.</param>
    [ScriptMethod]
    public void TrackSetPath(int trackIdx, NodePath path)
    {
        GDAnimation.TrackSetPath(trackIdx, path);
    }

    /// <summary>
    /// Swaps the track <paramref name="trackIdx"/>'s index position with the track <paramref name="withIdx"/>.
    /// </summary>
    /// <param name="trackIdx">The index of the first track.</param>
    /// <param name="withIdx">The index of the second track to swap with.</param>
    [ScriptMethod]
    public void TrackSwap(int trackIdx, int withIdx)
    {
        GDAnimation.TrackSwap(trackIdx, withIdx);
    }

    /// <summary>
    /// Returns the update mode of a value track.
    /// </summary>
    /// <param name="trackIdx">The index of the value track.</param>
    /// <returns>The update mode enum value.</returns>
    [ScriptMethod]
    public UpdateModeEnum ValueTrackGetUpdateMode(int trackIdx)
    {
        return (UpdateModeEnum)GDAnimation.ValueTrackGetUpdateMode(trackIdx);
    }

    /// <summary>
    /// Returns the interpolated value at the given time (in seconds). The <paramref name="trackIdx"/> must be the index of a value track.
    /// <para>A <paramref name="backward"/> mainly affects the direction of key retrieval of the track with <see cref="UpdateModeEnum.Discrete"/> converted by <see cref="AnimationMixer.AnimationCallbackModeDiscreteForceContinuous"/> to match the result with <see cref="TrackFindKey"/>.</para>
    /// </summary>
    /// <param name="trackIdx">The index of the value track.</param>
    /// <param name="timeSec">The time (in seconds) to interpolate.</param>
    /// <param name="backward">If true, searches backwards for the previous key if no exact match is found.</param>
    /// <returns>The interpolated value (as a Variant).</returns>
    [ScriptMethod]
    public Variant ValueTrackInterpolate(int trackIdx, float timeSec, bool backward = false)
    {
        return GDAnimation.ValueTrackInterpolate(trackIdx, timeSec, backward);
    }

    /// <summary>
    /// Sets the update mode of a value track.
    /// </summary>
    /// <param name="trackIdx">The index of the value track.</param>
    /// <param name="mode">The update mode to set.</param>
    [ScriptMethod]
    public void ValueTrackSetUpdateMode(int trackIdx, UpdateModeEnum mode)
    {
        GDAnimation.ValueTrackSetUpdateMode(trackIdx, (Godot.Animation.UpdateMode)mode);
    }

    public override void Init()
    {
        GDAnimation ??= new Godot.Animation();
        GDAnimations.Add(GDAnimation, this);
        base.Init();
    }

    public override void PreDelete()
    {
        GDAnimations.Remove(GDAnimation);
        base.PreDelete();
    }
}