// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Scripting;

namespace Polytoria.Datamodel.Services;

// TODO: Fix this service, uses old ClientSettings
/// <summary>
/// PreferencesService is a service that allows scripts to access some of the user preferences
/// </summary>
[Static("Preferences")]
[ExplorerExclude]
[SaveIgnore]
[DocCategory("services")]
public sealed partial class PreferencesService : Instance
{
	/// <summary>
	/// Fired when a user preference setting is changed.
	/// </summary>
	/// <param name="settingName" type="string"></param>
	/// <param name="setTo" type="any"></param>
	[ScriptProperty] public PTSignal<string, object> SettingChanged { get; private set; } = new();
	/// <summary>
	/// Determines whether the player has photo mode enabled.
	/// </summary>
	[ScriptProperty] public static bool UsePhotoMode => false;//ClientSettings.Singleton.Settings.PhotoMode;
	/// <summary>
	/// Determines whether the player has post-processing effects enabled.
	/// </summary>
	[ScriptProperty] public static bool UsePostProcessing => false;//ClientSettings.Singleton.Settings.PostProcessing;


	public override void Init()
	{
		// ClientSettings.Singleton.OnSettingChanged += OnSettingChanged;
		base.Init();
	}

	public override void PreDelete()
	{
		// ClientSettings.Singleton.OnSettingChanged -= OnSettingChanged;
		base.PreDelete();
	}
}
