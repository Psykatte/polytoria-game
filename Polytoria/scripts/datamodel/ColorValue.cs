// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// ColorValue is an object that holds a <see cref="Scripting.Datatypes.PTColor"/> value.
/// </summary>
[Instantiable]
[DocCategory("values")]
public partial class ColorValue : ValueBase
{
	private Color _val = new(1, 1, 1);

	/// <summary>
	/// The <see cref="Scripting.Datatypes.PTColor"/> value stored in this object.
	/// </summary>
	[Editable, ScriptProperty]
	public Color Value
	{
		get => _val;
		set
		{
			Color oldVal = _val;
			_val = value;
			if (_val != oldVal)
			{
				InvokeChanged();
			}
			OnPropertyChanged();
		}
	}
}
