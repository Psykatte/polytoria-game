// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// Vector3Value is an object that holds a Vector3 value.
/// </summary>
[Instantiable]
public partial class Vector3Value : ValueBase
{
	private Vector3 _val = new(0, 0, 0);

	/// <summary>
	/// The value of this object.
	/// </summary>
	[Editable, ScriptProperty]
	public Vector3 Value
	{
		get => _val;
		set
		{
			Vector3 oldVal = _val;
			_val = value;
			if (_val != oldVal)
			{
				InvokeChanged();
			}
			OnPropertyChanged();
		}
	}
}
