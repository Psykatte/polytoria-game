// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Shared.AssetLoaders;

namespace Polytoria.Datamodel.Resources;

/// <summary>
/// A mesh asset that's loaded from Polytoria.
/// </summary>
[Instantiable]
[DocCategory("assets")]
public partial class PTMeshAsset : MeshAsset
{
	private uint _assetID;

	/// <summary>
	/// Asset ID of this mesh
	/// </summary>
	[Editable, ScriptProperty]
	public uint AssetID
	{
		get => _assetID;
		set
		{
			_assetID = value;
			LoadResource();
			OnPropertyChanged();
		}
	}

	public static void RegisterAsset()
	{
		RegisterType<PTMeshAsset>();
	}

	public override void LoadResource()
	{
		AssetLoader.Singleton.GetResource(
			new() { Type = ResourceType.Mesh, ID = AssetID },
			InvokeResourceLoaded
		);
	}
}
