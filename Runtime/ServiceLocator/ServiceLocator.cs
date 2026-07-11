using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace dyvoid.Utility
{
	/// <summary>
	/// A static service locator that provides a centralized registry for application services.
	/// Each (Type, name) key holds a stack of registrations. Lookups return the most recently
	/// registered instance; removing a registration reveals the next-latest surviving one.
	/// </summary>
	public static class ServiceLocator
	{
		/// <summary>
		/// Internal store: each (Type, name) maps to a stack (List) of registered instances.
		/// The last element is the active registration; earlier elements are shadowed.
		/// </summary>
		static readonly ConcurrentDictionary<(Type, string), List<object>> _services = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics()
		{
			_services.Clear();
		}

		/// <summary>
		/// Registers a service instance for the specified type. Stacks on top of any existing
		/// registrations for the same (Type, name); the new instance becomes the active one.
		/// </summary>
		public static void Register<T>(T service, string name = null) where T : class
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			var key = (typeof(T), name);

			while (true)
			{
				var stack = _services.GetOrAdd(key, _ => new List<object>());

				lock (stack)
				{
					// The stack may have been pruned from the dictionary between GetOrAdd
					// and taking the lock; only add if it is still the registered stack.
					if (_services.TryGetValue(key, out var current) && ReferenceEquals(current, stack))
					{
						stack.Add(service);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Registers a service instance under a specific interface type.
		/// </summary>
		public static void Register<TInterface, TImplementation>(TImplementation service, string name = null)
			where TImplementation : class, TInterface
			where TInterface : class
		{
			Register<TInterface>(service, name);
		}

		/// <summary>
		/// Retrieves the currently active service of the specified type.
		/// Walks down the stack pruning dead UnityEngine.Object references.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when no live service is registered.</exception>
		public static T Get<T>(string name = null) where T : class
		{
			if (TryGet<T>(out var service, name)) return service;

			throw new InvalidOperationException($"Service of type {typeof(T).Name}{(name != null ? $" ({name})" : "")} not registered");
		}

		/// <summary>
		/// Attempts to retrieve the currently active service without throwing.
		/// Walks down the stack pruning dead UnityEngine.Object references.
		/// </summary>
		public static bool TryGet<T>(out T service, string name = null) where T : class
		{
			var key = (typeof(T), name);

			if (_services.TryGetValue(key, out var stack))
			{
				lock (stack)
				{
					for (int i = stack.Count - 1; i >= 0; i--)
					{
						var candidate = stack[i];

						// Prune dead UnityEngine.Object references.
						if (candidate is UnityEngine.Object unityObj && unityObj == null)
						{
							stack.RemoveAt(i);
							continue;
						}

						service = candidate as T;
						if (service != null) return true;
					}

					PruneIfEmpty(key, stack);
				}
			}

			service = default;
			return false;
		}

		/// <summary>
		/// Checks if a live service of the specified type is currently registered.
		/// </summary>
		public static bool Has<T>(string name = null) where T : class
		{
			return TryGet<T>(out _, name);
		}

		/// <summary>
		/// Removes a specific service instance from its registration stack by reference equality.
		/// Underlying registrations (if any) remain and become active again.
		/// </summary>
		public static void Unregister<T>(T instance, string name = null) where T : class
		{
			if (instance == null) return;
			UnregisterInternal(typeof(T), name, instance);
		}

		/// <summary>
		/// Non-generic instance-matched unregister used by ServiceBootstrap bookkeeping.
		/// </summary>
		internal static void UnregisterInternal(Type type, string name, object instance)
		{
			var key = (type, name);

			if (!_services.TryGetValue(key, out var stack)) return;

			lock (stack)
			{
				stack.Remove(instance);
				PruneIfEmpty(key, stack);
			}
		}

		/// <summary>
		/// Removes an emptied stack's dictionary entry so unused (Type, name) keys don't
		/// accumulate. Must be called while holding the stack's lock; the conditional remove
		/// only succeeds if the key still maps to this exact stack.
		/// </summary>
		static void PruneIfEmpty((Type, string) key, List<object> stack)
		{
			if (stack.Count != 0) return;

			((ICollection<KeyValuePair<(Type, string), List<object>>>)_services)
				.Remove(new KeyValuePair<(Type, string), List<object>>(key, stack));
		}

		/// <summary>
		/// Clears all registered services and disposes any that implement IDisposable.
		/// Each instance is disposed at most once even if registered under multiple keys.
		/// </summary>
		public static void Clear()
		{
			var disposed = new HashSet<object>(ReferenceComparer.Instance);

			foreach (var stack in _services.Values)
			{
				lock (stack)
				{
					foreach (var service in stack)
					{
						if (service is IDisposable disposable && disposed.Add(service))
							disposable.Dispose();
					}
				}
			}
			_services.Clear();
		}

		sealed class ReferenceComparer : IEqualityComparer<object>
		{
			public static readonly ReferenceComparer Instance = new();
			public new bool Equals(object x, object y) => ReferenceEquals(x, y);
			public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
		}
	}
}
