// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Scripting;
using System.Collections.Generic;

namespace Polytoria.Datamodel.Data;

/// <summary>
/// HttpRequestData represents the data required to construct an HTTP request.
/// </summary>
[DocCategory("types")]
public partial class HttpRequestData : IScriptObject
{
	/// <summary>
	/// The target endpoint of the HTTP request.
	/// </summary>
	[ScriptProperty] public string URL { get; set; } = "";

	/// <summary>
	/// The HTTP method used for the request.
	/// </summary>
	[ScriptProperty] public HttpRequestMethodEnum Method { get; set; } = HttpRequestMethodEnum.Get;

	/// <summary>
	/// The payload sent with the request.
	/// </summary>
	[ScriptProperty] public string? Body { get; set; }
	
	/// <summary>
	/// A table of HTTP headers to include with the request, represented as key-value pairs.
	/// </summary>
	[ScriptProperty] public Dictionary<string, string>? Headers { get; set; }

	/// <summary>
	/// Creates and returns a new instance of <c>HttpRequestData</c>.
	/// </summary>
	[ScriptMethod]
	public static HttpRequestData New()
	{
		return new();
	}

	[ScriptEnum]
	public enum HttpRequestMethodEnum
	{
		Get,
		Post,
		Put,
		Delete,
		Patch
	}
}
