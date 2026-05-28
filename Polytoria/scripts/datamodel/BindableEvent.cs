// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Scripting;

namespace Polytoria.Datamodel;

/// <summary>
/// BindableEvent is an event that can be called to communicate between scripts in the same boundary.
/// </summary>
[Instantiable]
[DocCategory("networking")]
public sealed partial class BindableEvent : Instance
{
	/// <summary>
	/// Fires when this event has been invoked.
	/// </summary>
	/// <param name="..." type="any"></param>
	[ScriptProperty] public PTSignal Invoked { get; private set; } = new();

	/// <summary>
	/// Invoke this event with parameters.
	/// </summary>
	[ScriptMethod]
	public void Invoke(params object?[] par)
	{
		Invoked.InvokeDirect(par);
	}
}
