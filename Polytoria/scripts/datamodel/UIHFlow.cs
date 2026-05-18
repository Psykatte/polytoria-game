// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// UIHFlow is a class that aligns all of it's children horizontally and wraps them around at the borders.
/// </summary>
[Instantiable]
public partial class UIHFlow : UIFlowLayout
{
	public override Node CreateGDNode()
	{
		return new HFlowContainer();
	}
}
