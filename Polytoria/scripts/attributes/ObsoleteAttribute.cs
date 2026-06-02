// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace Polytoria.Attributes;

/// <summary>
/// Marks a scripting member as obsolete. Document the reason and the
/// replacement in the member's <c>remarks</c> tag, leading with "OBSOLETE:",
/// which the docs generator renders as an "Obsolete API" banner.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class ObsoleteAttribute : Attribute;
