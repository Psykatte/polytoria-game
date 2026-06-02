// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// InstanceValue is an object that holds an <see cref="Instance"/> value.
/// </summary>
[Instantiable]
[DocCategory("values")]
public partial class InstanceValue : ValueBase
{
	private Instance? _val;

	/// <summary>
	/// The <see cref="Instance"/> value stored in this object.
	/// </summary>
	[Editable, ScriptProperty]
	public Instance? Value
	{
		get => _val;
		set
		{
			Instance? oldVal = _val;
			_val = value;
			if (_val != oldVal)
			{
				InvokeChanged();
			}
			OnPropertyChanged();
		}
	}
}
