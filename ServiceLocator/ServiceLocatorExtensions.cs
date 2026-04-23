using UnityEngine;

/// <summary>
/// Extension methods for MonoBehaviour to retrieve services from the ServiceLocator,
/// mirroring Unity's native GetComponent pattern.
/// </summary>
public static class ServiceLocatorExtensions
{
	/// <summary>
	/// Retrieves a registered service of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of service to retrieve</typeparam>
	/// <param name="_">The MonoBehaviour this is called on (unused)</param>
	/// <param name="name">Optional name of the service to retrieve</param>
	/// <returns>The registered service instance</returns>
	public static T GetService<T>(this MonoBehaviour _, string name = null) where T : class
		=> ServiceLocator.Get<T>(name);

	/// <summary>
	/// Attempts to retrieve a registered service without throwing an exception if not found.
	/// </summary>
	/// <typeparam name="T">The type of service to retrieve</typeparam>
	/// <param name="_">The MonoBehaviour this is called on (unused)</param>
	/// <param name="service">The retrieved service, or null if not found</param>
	/// <param name="name">Optional name of the service to retrieve</param>
	/// <returns>True if the service was found; otherwise, false</returns>
	public static bool TryGetService<T>(this MonoBehaviour _, out T service, string name = null) where T : class
		=> ServiceLocator.TryGet<T>(out service, name);
}
