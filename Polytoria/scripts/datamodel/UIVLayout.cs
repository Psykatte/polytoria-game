// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// UIVLayout is a class that aligns all of its children vertically.
/// </summary>
[Instantiable]
[DocCategory("ui")]
public partial class UIVLayout : UIHVLayout
{
	public override Node CreateGDNode()
	{
		return new VBoxContainer();
	}
}
