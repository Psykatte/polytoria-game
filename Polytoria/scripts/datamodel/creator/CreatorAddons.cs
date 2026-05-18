// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Creator.Managers;
using Polytoria.Creator.UI;
using Polytoria.Datamodel.Resources;
using Polytoria.Scripting;
using Polytoria.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polytoria.Datamodel.Creator;

/// <summary>
/// Service for managing addons
/// </summary>
[Static("Addons")]
public sealed partial class CreatorAddons : Instance
{
	private const float CleanupTimeout = 10;

	private readonly Dictionary<string, AddonObject> _identifierToAddon = [];
	private readonly Dictionary<Script, AddonObject> _scriptToAddon = [];

	/// <summary>
	/// Register an addon
	/// </summary>
	[ScriptMethod(Permissions = ScriptPermissionFlags.ContextAccess)]
	public AddonObject Register([ScriptingCaller] Script caller, string identifier)
	{
		if (_scriptToAddon.ContainsKey(caller))
		{
			throw new System.Exception("This script has already been registered");
		}
		if (_identifierToAddon.TryGetValue(identifier, out AddonObject? addonObject))
		{
			CleanupAddonObject(addonObject);
		}
		AddonObject obj = new() { Identifier = identifier, Root = Root, ScriptSource = caller };
		_identifierToAddon[identifier] = obj;
		_scriptToAddon.Add(caller, obj);
		return obj;
	}

	public override void PreDelete()
	{
		foreach (AddonObject addon in _identifierToAddon.Values)
		{
			CleanupAddonObject(addon);
		}
		_identifierToAddon.Clear();
		base.PreDelete();
	}

	private async void CleanupAddonObject(AddonObject obj)
	{
		Menu.Singleton.RemoveAddonMenu(obj);
		obj.CleanupReceived.Invoke();
		_scriptToAddon.Remove(obj.ScriptSource);

		// Wait for CleanupTimeout then stop the scripts
		await Globals.Singleton.WaitAsync(CleanupTimeout);
		obj.ScriptSource.ForceDelete();
	}

	/// <summary>
	/// AddonObject represents an addon in the creator. This object serves as the main interface for addon developers to interact with the addon system.
	/// </summary>
	public class AddonObject : IScriptObject
	{
		private string _addonName = "No name";
		private PTImageAsset? _addonIcon;

		public World Root = null!;
		public Script ScriptSource = null!;

		/// <summary>
		/// The identifier for this addon.
		/// </summary>
		[ScriptProperty] public string Identifier { get; internal set; } = "";
		/// <summary>
		/// Fires when cleanup has been requested by the creator, this is usually fired when updating the addon.
		/// </summary>
		[ScriptProperty] public PTSignal CleanupReceived { get; private set; } = new();

		/// <summary>
		/// The display name of the addon. This name will appear in the tools menu.
		/// </summary>
		[ScriptProperty]
		public string AddonName
		{
			get => _addonName;
			set
			{
				_addonName = value;
				Menu.Singleton.UpdateAddonMenu(this);
			}
		}

		/// <summary>
		/// Determines the addon icon.
		/// </summary>
		[ScriptProperty]
		public PTImageAsset? AddonIcon
		{
			get => _addonIcon;
			set
			{
				_addonIcon = value;
				Menu.Singleton.UpdateAddonMenu(this);
			}
		}

		public readonly List<AddonToolItem> ToolItems = [];

		/// <summary>
		/// Prompt the user to request for permissions
		/// </summary>
		[ScriptMethod]
		public static async Task RequestPermissions([ScriptingCaller] Script caller, AddonPermissionEnum[] perms)
		{
			var data = AddonsManager.GetAddonSession(caller);
			if (data != null)
			{
				if (AddonsManager.GetHasAskedForPerms(data.Path)) return;
				bool res = await CreatorService.Interface.PromptAddonReqPerm(perms, data.Data);

				if (res)
				{
					AddonsManager.SetAddonPermissions(caller, perms, true);
				}
				else
				{
					AddonsManager.SetAddonPermissions(caller, perms, false);
					throw new System.Exception("User declined permissions");
				}
			}
		}

		/// <summary>
		/// Create a new tool item with text.
		/// </summary>
		[ScriptMethod]
		public AddonToolItem CreateToolItem(string txt)
		{
			AddonToolItem item = new(txt);
			ToolItems.Add(item);
			Menu.Singleton.UpdateAddonMenu(this);
			return item;
		}
	}

	/// <summary>
	/// AddonToolItem represents a tool item in the tools menu of the addon.
	/// </summary>
	public class AddonToolItem(string txt) : IScriptObject
	{
		public string Text = txt;

		/// <summary>
		/// Fires when this tool item has been pressed
		/// </summary>
		[ScriptProperty] public PTSignal Pressed { get; private set; } = new();
	}

	[ScriptEnum(IsCreatorOnly = true)]
	public enum AddonPermissionEnum
	{
		IORead,
		IOWrite
	}
}
