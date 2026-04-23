using System;
using System.Collections.Concurrent;
using UnityEngine;

/// <summary>
/// A static service locator that provides a centralized registry for application services.
/// Allows registration, retrieval, and management of services by type and optional name.
/// </summary>
public static class ServiceLocator
{
	/// <summary>
	/// Internal dictionary storing all registered services. The key is a tuple of (Type, name).
	/// </summary>
	static readonly ConcurrentDictionary<(Type, string), object> _services = new();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void ResetStatics()
	{
		_services.Clear();
	}

	/// <summary>
	/// Registers a service instance for the specified type.
	/// </summary>
	/// <typeparam name="T">The type to register the service as</typeparam>
	/// <param name="service">The service instance to register</param>
	/// <param name="name">Optional name to distinguish between multiple services of the same type</param>
	/// <exception cref="ArgumentNullException">Thrown when service is null</exception>
	public static void Register<T>(T service, string name = null) where T : class
	{
		if (service == null) throw new ArgumentNullException(nameof(service));

		var key = (typeof(T), name);
		if (_services.ContainsKey(key))
			UnityEngine.Debug.LogWarning($"Service {typeof(T).Name}{(name != null ? $" ({name})" : "")} is being overwritten");

		_services[key] = service;
	}

	/// <summary>
	/// Registers a service instance for a specific interface type.
	/// </summary>
	/// <typeparam name="TInterface">The interface type to register the service as</typeparam>
	/// <typeparam name="TImplementation">The concrete implementation type of the service</typeparam>
	/// <param name="service">The service instance to register</param>
	/// <param name="name">Optional name to distinguish between multiple services of the same type</param>
	public static void Register<TInterface, TImplementation>(TImplementation service, string name = null)
		where TImplementation : class, TInterface
		where TInterface : class
	{
		Register<TInterface>(service, name);
	}

	/// <summary>
	/// Retrieves a registered service of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of service to retrieve</typeparam>
	/// <param name="name">Optional name of the service to retrieve</param>
	/// <returns>The registered service instance</returns>
	/// <exception cref="InvalidOperationException">Thrown when the requested service is not registered</exception>
	public static T Get<T>(string name = null) where T : class
	{
		if (_services.TryGetValue((typeof(T), name), out var service))
			return service as T;

		throw new InvalidOperationException($"Service of type {typeof(T).Name}{(name != null ? $" ({name})" : "")} not registered");
	}

	/// <summary>
	/// Attempts to retrieve a registered service without throwing an exception if not found.
	/// </summary>
	/// <typeparam name="T">The type of service to retrieve</typeparam>
	/// <param name="service">When this method returns, contains the service instance if found, or null if not found</param>
	/// <param name="name">Optional name of the service to retrieve</param>
	/// <returns>True if the service was found; otherwise, false</returns>
	public static bool TryGet<T>(out T service, string name = null) where T : class
	{
		if (_services.TryGetValue((typeof(T), name), out var obj))
		{
			service = obj as T;
			return service != null;
		}

		service = default;
		return false;
	}

	/// <summary>
	/// Checks if a service of the specified type is registered.
	/// </summary>
	/// <typeparam name="T">The type of service to check for</typeparam>
	/// <param name="name">Optional name of the service to check for</param>
	/// <returns>True if the service is registered; otherwise, false</returns>
	public static bool Has<T>(string name = null) where T : class
	{
		return _services.ContainsKey((typeof(T), name));
	}

	/// <summary>
	/// Unregisters a service of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of service to unregister</typeparam>
	/// <param name="name">Optional name of the service to unregister</param>
	public static void Unregister<T>(string name = null) where T : class
	{
		_services.TryRemove((typeof(T), name), out _);
	}

	/// <summary>
	/// Clears all registered services and disposes any that implement IDisposable.
	/// </summary>
	public static void Clear()
	{
		foreach (var service in _services.Values)
		{
			if (service is IDisposable disposable)
				disposable.Dispose();
		}
		_services.Clear();
	}
}
