// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// QuaternionValue is an object that holds a <see cref="Polytoria.Scripting.Datatypes.PTQuaternion"/> value.
/// </summary>
[Instantiable]
[DocCategory("values")]
public partial class QuaternionValue : ValueBase
{
	private Quaternion _val = new(0, 0, 0, 0);

	/// <summary>
	/// The <see cref="Scripting.Datatypes.PTQuaternion"/> value stored in this object.
	/// </summary>
	[Editable, ScriptProperty]
	public Quaternion Value
	{
		get => _val;
		set
		{
			Quaternion oldVal = _val;
			_val = value;
			if (_val != oldVal)
			{
				InvokeChanged();
			}
			OnPropertyChanged();
		}
	}
}
