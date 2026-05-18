// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Datamodel.Interfaces;

namespace Polytoria.Datamodel;

/// <summary>
/// Model is an instance that can hold other instances, and which transform affects its children.
/// </summary>
[Instantiable]
[DocCategory("world")]
public sealed partial class Model : Dynamic, IGroup { }
