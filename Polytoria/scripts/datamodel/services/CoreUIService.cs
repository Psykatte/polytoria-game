// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Client.UI;
using Polytoria.Shared;
using System.Threading.Tasks;

namespace Polytoria.Datamodel.Services;

/// <summary>
/// CoreUI is a static class that allows for the toggling of certain core GUI.
/// </summary>
[Static("CoreUI")]
[DocCategory("services")]
public sealed partial class CoreUIService : Instance
{
	private const string CoreUIPath = "res://scenes/client/ui/core_ui.tscn";

	private bool _useUserCard = true;
	private bool _useChat = true;
	private bool _useHealthBar = true;
	private bool _useLeaderboard = true;
	private bool _useHotBar = true;
	private bool _useBackpack = true;
	private bool _useMenuButton = true;
	private bool _useEmoteWheel = true;
	private bool _canRespawn = true;

	public CoreUIRoot CoreUI = null!;

	/// <summary>
	/// Determines whether or not the user card (in the upper right hand corner above the leaderboard) is visible.
	/// </summary>
	[Editable, ScriptProperty, ScriptLegacyProperty("UserCardEnabled")]
	public bool UseUserCard
	{
		get => _useUserCard;
		set { _useUserCard = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the chat box is visible.
	/// </summary>
	[Editable, ScriptProperty, ScriptLegacyProperty("ChatEnabled")]
	public bool UseChat
	{
		get => _useChat;
		set { _useChat = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the player's health bar is visible.
	/// </summary>
	[Editable, ScriptProperty, ScriptLegacyProperty("HealthBarEnabled")]
	public bool UseHealthBar
	{
		get => _useHealthBar;
		set { _useHealthBar = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the player list/leaderboard is visible.
	/// </summary>
	[Editable, ScriptProperty, ScriptLegacyProperty("LeaderboardEnabled")]
	public bool UseLeaderboard
	{
		get => _useLeaderboard;
		set { _useLeaderboard = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the hot bar is visible.
	/// </summary>
	[Editable, ScriptProperty, ScriptLegacyProperty("HotbarEnabled")]
	public bool UseHotbar
	{
		get => _useHotBar;
		set { _useHotBar = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the backpack is togglable.
	/// </summary>
	[Editable, ScriptProperty]
	public bool UseBackpack
	{
		get => _useBackpack;
		set { _useBackpack = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the menu button is visible.
	/// </summary>
	[Editable, ScriptProperty, ScriptLegacyProperty("MenuButtonEnabled")]
	public bool UseMenuButton
	{
		get => _useMenuButton;
		set { _useMenuButton = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the emote wheel is visible.
	/// </summary>
	[Editable, ScriptProperty]
	public bool UseEmoteWheel
	{
		get => _useEmoteWheel;
		set { _useEmoteWheel = value; RefreshCoreUIsVisibility(); OnPropertyChanged(); }
	}

	/// <summary>
	/// Determines whether or not the player can respawn.
	/// </summary>
	[Editable, ScriptProperty]
	public bool CanRespawn
	{
		get => _canRespawn;
		set { _canRespawn = value; OnPropertyChanged(); }
	}

	public override void Init()
	{
		Root.Loaded.Once(OnGameLoaded);

		base.Init();
	}

	private void RefreshCoreUIsVisibility()
	{
		if (CoreUI != null)
		{
			CoreUI.UserCard.Visible = UseUserCard;
			CoreUI.Chat.Visible = UseChat;
			CoreUI.ChatButton.Visible = UseChat;
			CoreUI.HealthBar.Visible = UseHealthBar;
			CoreUI.Leaderboard.Visible = UseLeaderboard;
			CoreUI.Inventory.Visible = UseHotbar;
			CoreUI.MenuButton.Visible = UseMenuButton;
			CoreUI.EmoteWheel.UseEmoteWheel = UseEmoteWheel;
		}
	}

	public override void Ready()
	{
		RefreshCoreUIsVisibility();
		base.Ready();
	}

	private void OnGameLoaded()
	{
		if (Root.Network.IsServer || Root.SessionType != World.SessionTypeEnum.Client) { return; }

		CoreUIRoot coreUI = Globals.CreateInstanceFromScene<CoreUIRoot>(CoreUIPath);
		coreUI.Root = Root;
		coreUI.Service = this;
		CoreUI = coreUI;
		GDNode.AddChild(coreUI, true, Godot.Node.InternalMode.Front);
		RefreshCoreUIsVisibility();
	}

	internal async Task<CoreUIRoot> WaitRoot()
	{
		if (CoreUI != null)
		{
			return CoreUI;
		}

		while (CoreUI == null)
		{
			await Task.Delay(100);
		}

		return CoreUI;
	}
}
