// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace Polytoria.Attributes;

/// <summary>
/// Groups this type under a documentation category. Used by the docs generator
/// to organize the API reference into sidebar sections (e.g. "services", "physics").
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DocCategoryAttribute(string category) : Attribute
{
	public string Category = category;
}