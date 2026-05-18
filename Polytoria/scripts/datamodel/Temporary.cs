// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// A temporary container. All class that were instantiated from <c>Instance.New</c> will have this class as their first parent.
/// </summary>
[Static, ExplorerExclude, SaveIgnore]
public sealed partial class Temporary : ServerHidden { }
