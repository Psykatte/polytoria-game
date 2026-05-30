// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace Polytoria.Scripting;

/// <summary>
/// Marker interface for objects that can be exposed to and accessed from scripts.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicEvents | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public interface IScriptObject
{

}
