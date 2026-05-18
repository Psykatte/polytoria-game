// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using MemoryPack;
using Polytoria.Attributes;
using Polytoria.Scripting;
using Polytoria.Utils;
using Polytoria.Utils.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polytoria.Datamodel.Data;

/// <summary>
/// Represents a network message used for communication between clients and servers.
/// </summary>
public partial class NetMessage : IScriptObject
{
	public Dictionary<string, string> Strings = [];
	public Dictionary<string, int> Ints = [];
	public Dictionary<string, float> Numbers = [];
	public Dictionary<string, bool> Bools = [];
	public Dictionary<string, Vector2> Vec2s = [];
	public Dictionary<string, Vector3> Vec3s = [];
	public Dictionary<string, Color> Colors = [];
	public Dictionary<string, Instance> Instances = [];
	public Dictionary<string, byte[]> Buffers = [];

	/// <summary>
	/// Adds a string value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddString(string key, string value)
	{
		Strings.Add(key, value);
	}

	/// <summary>
	/// Adds an integer value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddInt(string key, int value)
	{
		Ints.Add(key, value);
	}

	/// <summary>
	/// Adds a boolean value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddBool(string key, bool value)
	{
		Bools.Add(key, value);
	}

	/// <summary>
	/// Adds a number value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddNumber(string key, float value)
	{
		Numbers.Add(key, value);
	}

	/// <summary>
	/// Adds a Vector2 value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddVector2(string key, Vector2 value)
	{
		Vec2s.Add(key, value);
	}

	/// <summary>
	/// Adds a Vector3 value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddVector3(string key, Vector3 value)
	{
		Vec3s.Add(key, value);
	}

	/// <summary>
	/// Adds a Color value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddColor(string key, Color value)
	{
		Colors.Add(key, value);
	}

	/// <summary>
	/// Adds an Instance value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddInstance(string key, Instance value)
	{
		Instances.Add(key, value);
	}

	/// <summary>
	/// Adds an Buffer value to the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public void AddBuffer(string key, byte[] buffer)
	{
		Buffers.Add(key, buffer);
	}

	/// <summary>
	/// Gets a string value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public string? GetString(string key) => Strings.TryGetValue(key, out var value) ? value : null;

	/// <summary>
	/// Gets an integer value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public int? GetInt(string key) => Ints.TryGetValue(key, out var value) ? value : (int?)null;

	/// <summary>
	/// Gets a number value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public float? GetNumber(string key) => Numbers.TryGetValue(key, out var value) ? value : (float?)null;

	/// <summary>
	/// Gets a boolean value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public bool? GetBool(string key) => Bools.TryGetValue(key, out var value) ? value : (bool?)null;

	/// <summary>
	/// Gets a Vector2 value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public Vector2? GetVector2(string key) => Vec2s.TryGetValue(key, out var value) ? value : (Vector2?)null;

	/// <summary>
	/// Gets a Vector3 value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public Vector3? GetVector3(string key) => Vec3s.TryGetValue(key, out var value) ? value : (Vector3?)null;

	/// <summary>
	/// Gets a Color value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public Color? GetColor(string key) => Colors.TryGetValue(key, out var value) ? value : (Color?)null;

	/// <summary>
	/// Gets an Instance value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public Instance? GetInstance(string key) => Instances.TryGetValue(key, out var value) ? value : null;

	/// <summary>
	/// Gets an Buffer value from the message with the specified key.
	/// </summary>
	[ScriptMethod]
	public byte[]? GetBuffer(string key) => Buffers.TryGetValue(key, out var value) ? value : null;

	/// <summary>
	/// Creates a new NetMessage instance.
	/// </summary>
	[ScriptMethod]
	public static NetMessage New()
	{
		return new NetMessage();
	}

	public byte[] Serialize()
	{
		NetMessagePayload payload = new()
		{
			Strings = Strings,
			Ints = Ints,
			Numbers = Numbers,
			Bools = Bools,
			Buffers = Buffers,
		};
		foreach ((string key, Vector2 v2) in Vec2s)
		{
			payload.Vec2s[key] = new Vector2Dto(v2);
		}
		foreach ((string key, Vector3 v3) in Vec3s)
		{
			payload.Vec3s[key] = new Vector3Dto(v3);
		}
		foreach ((string key, Color c) in Colors)
		{
			payload.Colors[key] = new ColorDto(c);
		}
		foreach ((string key, Instance i) in Instances)
		{
			payload.Instances[key] = i.NetworkedObjectID;
		}
		return SerializeUtils.Serialize(payload);
	}

	public static async Task<NetMessage> Deserialize(byte[] rawdata)
	{
		NetMessagePayload? payload = SerializeUtils.Deserialize<NetMessagePayload>(rawdata) ?? throw new Exception("Message is invalid");
		NetMessage msg = new()
		{
			Strings = payload.Strings,
			Ints = payload.Ints,
			Numbers = payload.Numbers,
			Bools = payload.Bools,
			Buffers = payload.Buffers,
		};
		foreach ((string key, Vector2Dto v2) in payload.Vec2s)
		{
			msg.Vec2s[key] = v2.ToVector2();
		}
		foreach ((string key, Vector3Dto v3) in payload.Vec3s)
		{
			msg.Vec3s[key] = v3.ToVector3();
		}
		foreach ((string key, ColorDto c) in payload.Colors)
		{
			msg.Colors[key] = c.ToColor();
		}
		foreach ((string key, string netID) in payload.Instances)
		{
			NetworkedObject? netobj = await World.Current!.WaitForNetObjectAsync(netID);
			if (netobj != null && netobj is Instance i)
			{
				msg.Instances[key] = i;
			}
		}
		return msg;
	}

	[MemoryPackable]
	public partial class NetMessagePayload
	{
		public Dictionary<string, string> Strings = [];
		public Dictionary<string, int> Ints = [];
		public Dictionary<string, float> Numbers = [];
		public Dictionary<string, bool> Bools = [];
		public Dictionary<string, Vector2Dto> Vec2s = [];
		public Dictionary<string, Vector3Dto> Vec3s = [];
		public Dictionary<string, ColorDto> Colors = [];
		public Dictionary<string, string> Instances = [];
		public Dictionary<string, byte[]> Buffers = [];
	}
}
