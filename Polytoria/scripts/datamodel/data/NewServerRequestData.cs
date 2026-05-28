// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Scripting;

namespace Polytoria.Datamodel.Data;

/// <summary>
/// NewServerRequestData represents the request data for a new server instance, to be used with WorldsService.
/// </summary>
[DocCategory("misc")]
public partial class NewServerRequestData : IScriptObject
{
	[ScriptProperty] public string WorldPath { get; set; } = "";
	
	/// <summary>
	/// How much player slot should the new server allocates for.
	/// </summary>
	[ScriptProperty] public int MaxPlayers { get; set; } = 12;

	/// <summary>
	/// Creates a new <c>NewServerRequestData</c>.
	/// </summary>
	[ScriptMethod]
	public static NewServerRequestData New()
	{
		return new();
	}
}
