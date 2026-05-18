// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// IntValue is an object that holds an integer value.
/// </summary>
[Instantiable]
public partial class IntValue : ValueBase
{
	private int _val = 0;

	/// <summary>
	/// The value of this object.
	/// </summary>
	[Editable, ScriptProperty, DefaultValue(0)]
	public int Value
	{
		get => _val;
		set
		{
			int oldVal = _val;
			_val = value;
			if (_val != oldVal)
			{
				InvokeChanged();
			}
			OnPropertyChanged();
		}
	}
}
