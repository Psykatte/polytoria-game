// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// StringValue is an object that holds a string value.
/// </summary>
[Instantiable]
[DocCategory("values")]
public partial class StringValue : ValueBase
{
	private string _val = "";

	/// <summary>
	/// The value of this object.
	/// </summary>
	[Editable, ScriptProperty]
	public string Value
	{
		get => _val;
		set
		{
			string oldVal = _val;
			_val = value;
			if (_val != oldVal)
			{
				InvokeChanged();
			}
			OnPropertyChanged();
		}
	}
}
