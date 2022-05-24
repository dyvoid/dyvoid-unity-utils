using UnityEngine;

namespace References.Components
{
	public interface IRunInstaller
	{
		void RunInstaller();
	}

	/// <summary>
	/// Runs attached components that implement IRunInstaller before the regular Awake events in the scene.
	/// </summary>
	public class RunInstallersEarly : MonoBehaviour
	{
		private void Awake()
		{
			var attachedInstallers = GetComponents<IRunInstaller>();

			foreach (var installer in attachedInstallers)
			{
				installer.RunInstaller();
			}
		}
	}
}