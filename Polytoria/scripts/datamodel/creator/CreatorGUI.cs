// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel.Creator;

/// <summary>
/// CreatorGUI is an object that allows GUI to overlay on top of the viewport in the creator. This class is only available in the creator.
/// </summary>
[ExplorerExclude, SaveIgnore]
public sealed partial class CreatorGUI : Instance
{
	public override Node CreateGDNode()
	{
		return new CanvasLayer() { Layer = 2000 };
	}
}
