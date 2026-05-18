// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// ModuleScripts are specialized scripts to hold data that can be accessed by other scripts using the require() function. It is important to define and return a table in a ModuleScript. When the place starts, the server and the client will run the ModuleScript once and store the result for other scripts to retrieve with require().
/// </summary>
[Instantiable]
public sealed partial class ModuleScript : Script
{
	internal int? CachedLuauResultRef { get; set; } = null;

	public override void EnterTree()
	{
		CheckSource();
		base.EnterTree();
	}

	internal void CheckSource()
	{
		if (!Root.Network.IsServer)
		{
			if (Source == "" && Root.IsLoaded)
			{
				RequestSource();
			}
		}
	}


	private void RequestSource()
	{
		Root.Network.ScriptSync.RequestSource(this);
	}
}
