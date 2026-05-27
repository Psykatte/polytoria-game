// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Shared.AssetLoaders;

namespace Polytoria.Datamodel.Resources;

/// <summary>
/// Audio asset which is loaded from Polytoria
/// </summary>
[Instantiable]
[DocCategory("assets")]
public partial class PTAudioAsset : AudioAsset
{
	private uint _audioID = 0;

	/// <summary>
	/// The audio ID to load
	/// </summary>
	[Editable, ScriptProperty]
	public uint AudioID
	{
		get => _audioID;
		set
		{
			_audioID = value;
			LoadResource();
			OnPropertyChanged();
		}
	}

	public static void RegisterAsset()
	{
		RegisterType<PTAudioAsset>();
	}

	public override void LoadResource()
	{
		if (AudioID == 0) return;
		AssetLoader.Singleton.GetResource(
			new() { Type = ResourceType.Audio, ID = AudioID },
			InvokeResourceLoaded
		);
	}
}
