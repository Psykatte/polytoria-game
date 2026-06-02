// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// Vector2Value is an object that holds a <see cref="Scripting.Datatypes.PTVector2"/> value.
/// </summary>
[Instantiable]
[DocCategory("values")]
public partial class Vector2Value : ValueBase
{
	private Vector2 _val = new(0, 0);

	/// <summary>
	/// The <see cref="Scripting.Datatypes.PTVector2"/> value stored in this object.
	/// </summary>
	[Editable, ScriptProperty]
	public Vector2 Value
	{
		get => _val;
		set
		{
			Vector2 oldVal = _val;
			_val = value;
			if (_val != oldVal)
			{
				InvokeChanged();
			}
			OnPropertyChanged();
		}
	}
}
