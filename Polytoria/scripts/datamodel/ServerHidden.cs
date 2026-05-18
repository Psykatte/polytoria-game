// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;

namespace Polytoria.Datamodel;

/// <summary>
/// ServerHidden, similar to Hidden, is a container for objects that are meant to be hidden. Unlike Hidden, ServerHidden won't replicate its contents to clients and can only be accessed by the server.
/// </summary>
[Static("ServerHidden")]
public partial class ServerHidden : HiddenBase { }
