// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Polytoria.Attributes;
using Polytoria.Datamodel.Services;
using Polytoria.Providers.Datastore;
using Polytoria.Scripting;
using System.Threading.Tasks;

namespace Polytoria.Datamodel.Data;

/// <summary>
/// Datastore is an object that represent datastore connection.
/// </summary>
[DocCategory("scripting")]
public partial class Datastore : IScriptObject
{
	private string _dsKey = null!;
	public DatastoreService DatastoreService { get; set; } = null!;

	public IDatastoreProvider Provider { get; set; } = null!;

	/// <summary>
	/// Returns whether the datastore is still loading.
	/// </summary>
	[ScriptLegacyProperty("Loading")] public bool LegacyLoading { get; private set; } = true;

	/// <summary>
	/// Fires when the datastore has finished loading.
	/// </summary>
	[ScriptLegacyProperty("Loaded")] public PTSignal LegacyLoaded { get; private set; } = new();

	/// <summary>
	/// The key identifying this Datastore connection.
	/// </summary>
	[ScriptProperty]
	public string Key => _dsKey;

	public void Connect(string key, IDatastoreProvider provider)
	{
		_dsKey = key;
		Provider = provider;
		Provider.Connect(key, this);
		LegacyLoading = false;
		LegacyLoaded.Invoke();
	}

	/// <summary>
	/// Retrieves a value from the datastore asynchronously using the specified key.
	/// </summary>
	[ScriptMethod]
	public async Task<object?> GetAsync(string key)
	{
		return await Provider.ReadData(key);
	}

	/// <summary>
	/// Stores a value in the datastore asynchronously using the specified key.
	/// </summary>
	[ScriptMethod]
	public async Task SetAsync(string key, object value)
	{
		await Provider.WriteData(key, value);
	}

	/// <summary>
	/// Removes a value from the datastore asynchronously using the specified key.
	/// </summary>
	[ScriptMethod]
	public async Task RemoveAsync(string key)
	{
		await Provider.WriteData(key, null);
	}

	/// <summary>
	/// Retrieves a value from the datastore by key and invokes the callback with the result.
	/// <remarks>Callback-based wrapper for <see cref="GetAsync"/>.</remarks>
	/// </summary>
	[ScriptLegacyMethod(nameof(Get))]
	public void Get(string key, PTCallback? callback)
	{
		_ = GetAsync(key).ContinueWith(tsk =>
		{
			if (tsk.IsCompletedSuccessfully)
			{
				object? val = tsk.Result;
				callback?.Invoke(val, true, null);
			}
			else
			{
				callback?.Invoke(null, false, tsk.Exception?.Message);
			}
		});
	}

	/// <summary>
	/// Stores a value in the datastore by key and invokes the callback on completion.
	/// <remarks>Callback-based wrapper for <see cref="SetAsync"/>.</remarks>
	/// </summary>
	[ScriptLegacyMethod(nameof(Set))]
	public void Set(string key, object value, PTCallback? callback)
	{
		_ = SetAsync(key, value).ContinueWith(tsk =>
		{
			if (tsk.IsCompletedSuccessfully)
			{
				callback?.Invoke(true);
			}
			else
			{
				callback?.Invoke(false, tsk.Exception?.Message);
			}
		});
	}

	/// <summary>
	/// Removes a value from the datastore by key and invokes the callback on completion.
	/// <remarks>Callback-based wrapper for <see cref="RemoveAsync"/>.</remarks>
	/// </summary>
	[ScriptLegacyMethod(nameof(Remove))]
	public void Remove(string key, PTCallback? callback)
	{
		_ = RemoveAsync(key).ContinueWith(tsk =>
		{
			if (tsk.IsCompletedSuccessfully)
			{
				callback?.Invoke(true);
			}
			else
			{
				callback?.Invoke(false, tsk.Exception?.Message);
			}
		});
	}

	/// <summary>
	/// Disconnect this datastore connection, this should be called when you finish using the datastore.
	/// </summary>
	[ScriptMethod]
	public void Disconnect()
	{
		Provider.Dispose();
		LegacyLoaded.DisconnectAll();
	}
}
