using System;
using System.Collections.Generic;
using UnityEngine;

namespace dyvoid.Utility
{
	/// <summary>
	/// Base class for bootstrapping services. Subclasses override <see cref="Install"/> and call
	/// <see cref="Bind{T}(T, string)"/> to register services. All bindings are automatically removed
	/// from the <see cref="ServiceLocator"/> when this MonoBehaviour is destroyed, using
	/// instance-matched removal so siblings remain unaffected.
	/// </summary>
	/// <remarks>
	/// Toggle <see cref="isGlobal"/> to make the bootstrap survive scene loads. Only one global
	/// bootstrap may be active at a time; duplicates self-destruct in Awake before registering.
	/// Place a global bootstrap in every scene that should be runnable standalone; the duplicate
	/// guard handles the transition automatically.
	/// </remarks>
	[DefaultExecutionOrder(-1000)]
	public abstract class ServiceBootstrap : MonoBehaviour
	{
		[Tooltip("If enabled, this bootstrap survives scene loads (DontDestroyOnLoad). Only one global bootstrap may be active at a time; duplicates self-destruct on Awake.")]
		[SerializeField] private bool isGlobal = false;

		static ServiceBootstrap _globalInstance;

		readonly List<Registration> _registrations = new();

		/// <summary>
		/// The currently active global bootstrap, or null if none is registered.
		/// </summary>
		public static ServiceBootstrap GlobalInstance => _globalInstance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics()
		{
			_globalInstance = null;
		}

		void Awake()
		{
			if (isGlobal)
			{
				if (_globalInstance != null && _globalInstance != this)
				{
					Debug.LogWarning($"[ServiceBootstrap] Global bootstrap already exists on '{_globalInstance.gameObject.name}'. Destroying duplicate on '{gameObject.name}'.");
					Destroy(gameObject);
					return;
				}

				_globalInstance = this;
				DontDestroyOnLoad(gameObject);
			}

			try
			{
				Install();
			}
			catch
			{
				// Unwind any partial registrations before propagating, so a failed
				// Install doesn't leave orphan services registered for the rest of play mode.
				UnwindRegistrations();
				if (this == _globalInstance) _globalInstance = null;
				throw;
			}
		}

		void OnDestroy()
		{
			if (this == _globalInstance) _globalInstance = null;

			try
			{
				OnBootstrapDestroy();
			}
			finally
			{
				UnwindRegistrations();
			}
		}

		/// <summary>
		/// Override for custom teardown. Runs before bindings are unregistered, so registered
		/// services are still resolvable here. Cleanup of bindings is automatic and cannot be skipped.
		/// </summary>
		protected virtual void OnBootstrapDestroy() { }

		void UnwindRegistrations()
		{
			// Unwind in reverse order so cleanup mirrors registration order.
			for (int i = _registrations.Count - 1; i >= 0; i--)
			{
				var reg = _registrations[i];
				ServiceLocator.UnregisterInternal(reg.Type, reg.Name, reg.Instance);
			}
			_registrations.Clear();
		}

		/// <summary>
		/// Override this method to register services via <see cref="Bind{T}(T, string)"/>.
		/// </summary>
		/// <example>
		/// <code>
		/// protected override void Install()
		/// {
		///     Bind&lt;ISaveSystem&gt;(new SaveSystem());
		///     Bind&lt;IAudioManager, AudioManager&gt;(audioManager);
		///     Bind&lt;IWeapon&gt;(primaryWeapon, WeaponNames.Primary);
		/// }
		/// </code>
		/// </example>
		protected abstract void Install();

		/// <summary>
		/// Registers a service and tracks it for automatic cleanup when this bootstrap is destroyed.
		/// </summary>
		protected void Bind<T>(T service, string name = null) where T : class
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			ServiceLocator.Register<T>(service, name);
			_registrations.Add(new Registration(typeof(T), name, service));
		}

		/// <summary>
		/// Registers a service under an interface type and tracks it for automatic cleanup.
		/// </summary>
		protected void Bind<TInterface, TImplementation>(TImplementation service, string name = null)
			where TImplementation : class, TInterface
			where TInterface : class
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			ServiceLocator.Register<TInterface, TImplementation>(service, name);
			_registrations.Add(new Registration(typeof(TInterface), name, service));
		}

		readonly struct Registration
		{
			public readonly Type Type;
			public readonly string Name;
			public readonly object Instance;

			public Registration(Type type, string name, object instance)
			{
				Type = type;
				Name = name;
				Instance = instance;
			}
		}
	}
}
