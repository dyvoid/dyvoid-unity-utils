#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Utility
{
	/// <summary>
	/// A utility class to manage and manipulate the time scale in Unity by pressing 0-9.
	/// Press 'p' for pause.
	/// </summary>
	public class TimeScaler : MonoBehaviour
	{
		[SerializeField] private bool requireLeftShift = true;
		[SerializeField] private bool useNumberKeys = true;

		private float prevTimeScale = 1.0f;
		private Dictionary<int, float> _timeScaleMap;

		/// <summary>
		/// Initializes the time scale map with predefined values.        
		/// </summary>
		private void Awake()
		{
			// Customize according to your needs
			_timeScaleMap = new Dictionary<int, float>
			{
				{ 0, 0.0f },
				{ 1, 0.01f },
				{ 2, 0.1f },
				{ 3, 0.25f },
				{ 4, 0.5f },
				{ 5, 1.0f },
				{ 6, 2.0f },
				{ 7, 5.0f },
				{ 8, 10.0f },
				{ 9, 25.0f }
			};
		}

		/// <summary>
		/// Updates the time scale based on input from the user.
		/// </summary>
		void Update()
		{
#if ENABLE_LEGACY_INPUT_MANAGER
			if (!requireLeftShift || Input.GetKey(KeyCode.LeftShift))
			{
				// Check numpad keys 0-9 or regular number keys based on setting
				for (int i = 0; i <= 9; i++)
				{
					if ((useNumberKeys && Input.GetKeyDown(KeyCode.Alpha0 + i)) || 
						(!useNumberKeys && Input.GetKeyDown(KeyCode.Keypad0 + i)))
					{
						Time.timeScale = _timeScaleMap[i];
						break;
					}
				}

				// Handle pause toggle
				if (Input.GetKeyDown(KeyCode.P))
				{
					TogglePause();
				}
			}
#elif ENABLE_INPUT_SYSTEM
			if (!requireLeftShift || Keyboard.current.leftShiftKey.isPressed)
			{
				var keys = useNumberKeys ? new[]
				{
					Keyboard.current.digit0Key, Keyboard.current.digit1Key, Keyboard.current.digit2Key,
					Keyboard.current.digit3Key, Keyboard.current.digit4Key, Keyboard.current.digit5Key,
					Keyboard.current.digit6Key, Keyboard.current.digit7Key, Keyboard.current.digit8Key,
					Keyboard.current.digit9Key
				} : new[]
				{
					Keyboard.current.numpad0Key, Keyboard.current.numpad1Key, Keyboard.current.numpad2Key,
					Keyboard.current.numpad3Key, Keyboard.current.numpad4Key, Keyboard.current.numpad5Key,
					Keyboard.current.numpad6Key, Keyboard.current.numpad7Key, Keyboard.current.numpad8Key,
					Keyboard.current.numpad9Key
				};

				for (int i = 0; i < keys.Length; i++)
				{
					if (keys[i].wasPressedThisFrame)
					{
						Time.timeScale = _timeScaleMap[i];
						break;
					}
				}

				// Handle pause toggle
				if (Keyboard.current.pKey.wasPressedThisFrame)
				{
					TogglePause();
				}
			}
#endif
		}

		/// <summary>
		/// Toggles the pause state of the game by setting the time scale to 0 or reverting to the previous time scale.
		/// </summary>
		private void TogglePause()
		{
			if (Time.timeScale == 0.0f)
			{
				Time.timeScale = prevTimeScale;
			}
			else
			{
				prevTimeScale = Time.timeScale;
				Time.timeScale = 0.0f;
			}
		}
	}
}
#endif
