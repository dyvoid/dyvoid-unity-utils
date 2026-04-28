using UnityEngine;

namespace Utility
{
	/// <summary>
	/// Base class for bootstrapping services in a Unity application.
	/// Provides a standardized way to initialize and register services with the ServiceLocator.
	/// Executes early in the Unity lifecycle to ensure services are available when needed.
	/// </summary>
	[DefaultExecutionOrder(-1000)]
	public abstract class ServiceBootstrap : MonoBehaviour
	{
		[SerializeField] protected bool initializeOnAwake = true;
		
		void Awake()
		{
			if (initializeOnAwake)
				Initialize();
		}
		
		/// <summary>
		/// Initializes the bootstrap by registering all services.
		/// Can be called manually if initializeOnAwake is false.
		/// </summary>
		public void Initialize()
		{
			RegisterServices();
		}

		/// <summary>
		/// Override this method to register services with the ServiceLocator.
		/// This is where service instances should be created and registered.
		/// </summary>
		/// <example>
		/// <code>
		/// protected override void RegisterServices()
		/// {
		///     // Register by type
		///     ServiceLocator.Register<PlayerController>(playerController);
		///     
		///     // Register with interface
		///     var saveSystem = new SaveSystem();
		///     ServiceLocator.Register<ISaveSystem>(saveSystem);
		///     
		///     // Register with name (for multiple instances of same type)
		///     ServiceLocator.Register<IWeapon>(primaryWeapon, "primary");
		///     ServiceLocator.Register<IWeapon>(secondaryWeapon, "secondary");
		/// }
		/// </code>
		/// </example>
		protected abstract void RegisterServices();
		
		public void OnDestroy()
		{
			OnBootstrapDestroy();
		}
		
		/// <summary>
		/// Override this method to perform cleanup when the bootstrap is destroyed.
		/// This is where you should unregister services or perform other cleanup tasks.
		/// </summary>
		protected virtual void OnBootstrapDestroy()
		{
			// Override for cleanup if needed
		}
	}
}
