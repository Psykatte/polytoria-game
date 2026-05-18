// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// Trusses are parts that can be climbed by the player.
/// </summary>
[Instantiable]
public sealed partial class Truss : Part
{
	private float _climbSpeed;

	/// <summary>
	/// The speed at which the player can climb the truss.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(1f)]
	public float ClimbSpeed
	{
		get => _climbSpeed;
		set
		{
			_climbSpeed = value;
			OnPropertyChanged();
		}
	}
}
