using UnityEngine;

namespace References.Components
{
	/// <summary>
	/// Base class for running scene installations.
	/// </summary>
	public abstract class SceneInstaller : MonoBehaviour, IRunInstaller
	{
		/// <summary>
		/// Called during Awake event. Attach a RunInstallersEarly component to ensure Install is called before the regular Awake events.
		/// </summary>
		public abstract void Install();

		/// <summary>
		/// Called during OnDestroy event.
		/// </summary>
		public abstract void Uninstall();

		public bool IsInstalled { get; private set; }

		public void RunInstaller()
		{
			if (IsInstalled) return;
			// Debug.Log($"Install: {GetType().Name} - {name}", this);
			Install();
			IsInstalled = true;
		}

		private void Awake()
		{
			RunInstaller();
		}

		private void OnDestroy()
		{
			if (IsInstalled)
			{
				Uninstall();
			}
		}
	}
}