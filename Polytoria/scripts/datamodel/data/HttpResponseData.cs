// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Scripting;
using System.Collections.Generic;
using System.Net.Http;

namespace Polytoria.Datamodel.Data;

/// <summary>
/// HttpResponseData represents the result of an HTTP request.
/// </summary>
public partial class HttpResponseData : IScriptObject
{
	/// <summary>
	/// Indicates whether the HTTP request completed successfully.
	/// </summary>
	[ScriptProperty] public bool Success { get; internal set; }
	/// <summary>
	/// The HTTP status code returned by the server.
	/// </summary>
	[ScriptProperty] public int StatusCode { get; internal set; }
	/// <summary>
	/// A table containing the HTTP response headers returned by the server, represented as key-value pairs.
	/// </summary>
	[ScriptProperty] public Dictionary<string, string>? Headers { get; internal set; }
	/// <summary>
	/// The response payload returned by the server as a string.
	/// </summary>
	[ScriptProperty] public string Body { get; internal set; } = "";
	/// <summary>
	/// The response payload returned by the server as a buffer.
	/// </summary>
	[ScriptProperty] public byte[] Buffer { get; internal set; } = [];

	internal HttpResponseMessage responseMsg = null!;
}
