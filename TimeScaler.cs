#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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
		private bool isPaused;
		private Dictionary<int, float> _timeScaleMap;
#if ENABLE_INPUT_SYSTEM
		private KeyControl[] _digitKeys;
		private KeyControl[] _numpadKeys;
#endif

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

#if ENABLE_INPUT_SYSTEM
			var kb = Keyboard.current;
			_digitKeys = new[]
			{
				kb.digit0Key, kb.digit1Key, kb.digit2Key, kb.digit3Key, kb.digit4Key,
				kb.digit5Key, kb.digit6Key, kb.digit7Key, kb.digit8Key, kb.digit9Key
			};
			_numpadKeys = new[]
			{
				kb.numpad0Key, kb.numpad1Key, kb.numpad2Key, kb.numpad3Key, kb.numpad4Key,
				kb.numpad5Key, kb.numpad6Key, kb.numpad7Key, kb.numpad8Key, kb.numpad9Key
			};
#endif
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
						SetTimeScale(_timeScaleMap[i]);
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
				KeyControl[] keys = useNumberKeys ? _digitKeys : _numpadKeys;

				for (int i = 0; i < keys.Length; i++)
				{
					if (keys[i].wasPressedThisFrame)
					{
						SetTimeScale(_timeScaleMap[i]);
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
		/// Applies a new time scale and keeps the pause state in sync.
		/// </summary>
		private void SetTimeScale(float scale)
		{
			Time.timeScale = scale;
			isPaused = (scale == 0f);

			if (!isPaused)
			{
				prevTimeScale = scale;
			}
		}

		/// <summary>
		/// Toggles the pause state of the game by setting the time scale to 0 or reverting to the previous time scale.
		/// </summary>
		private void TogglePause()
		{
			if (isPaused)
			{
				Time.timeScale = prevTimeScale;
				isPaused = false;
			}
			else
			{
				prevTimeScale = Time.timeScale;
				Time.timeScale = 0.0f;
				isPaused = true;
			}
		}
	}
}
#endif
