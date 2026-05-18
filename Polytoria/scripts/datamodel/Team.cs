// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using System.Collections.Generic;

namespace Polytoria.Datamodel;

/// <summary>
/// Team is an object that represents a player team to which players can be assigned.
/// </summary>
[Instantiable]
public partial class Team : Instance
{
	private string _displayName = "";
	private Color _color = new(1, 0, 0);

	/// <summary>
	/// Display name for this team
	/// </summary>
	[Editable, ScriptProperty, DefaultValue("")]
	public string DisplayName
	{
		get => _displayName;
		set
		{
			_displayName = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Color for this team
	/// </summary>
	[Editable, ScriptProperty]
	public Color Color
	{
		get => _color;
		set
		{
			_color = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Returns the display name of the team. If DisplayName is specified, it returns DisplayName; otherwise, it returns Name.
	/// </summary>
	[ScriptMethod]
	public string GetDisplayName()
	{
		return _displayName == string.Empty ? Name : _displayName;
	}

	/// <summary>
	/// Get all players assigned to this team.
	/// </summary>
	[ScriptMethod]
	public Player[] GetPlayers()
	{
		List<Player> plr = [];
		foreach (var item in Root.Players.GetPlayers())
		{
			if (item.Team == this)
			{
				plr.Add(item);
			}
		}
		return [.. plr];
	}
}
