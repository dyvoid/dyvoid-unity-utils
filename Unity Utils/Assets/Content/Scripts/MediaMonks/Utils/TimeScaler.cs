#if UNITY_EDITOR

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MediaMonks.Utility
{
    public class TimeScaler : MonoBehaviour
    {
        [SerializeField] private bool _requireLeftShift = true;

        private float _prevTimeScale = 1.0f;

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!_requireLeftShift || Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.Keypad0)) Time.timeScale = 0f;
                if (Input.GetKeyDown(KeyCode.Keypad1)) Time.timeScale = 0.01f;
                if (Input.GetKeyDown(KeyCode.Keypad2)) Time.timeScale = 0.1f;
                if (Input.GetKeyDown(KeyCode.Keypad3)) Time.timeScale = 0.25f;
                if (Input.GetKeyDown(KeyCode.Keypad4)) Time.timeScale = 0.5f;
                if (Input.GetKeyDown(KeyCode.Keypad5)) Time.timeScale = 1f;
                if (Input.GetKeyDown(KeyCode.Keypad6)) Time.timeScale = 2f;
                if (Input.GetKeyDown(KeyCode.Keypad7)) Time.timeScale = 5f;
                if (Input.GetKeyDown(KeyCode.Keypad8)) Time.timeScale = 10f;
                if (Input.GetKeyDown(KeyCode.Keypad9)) Time.timeScale = 25f;

                if (Input.GetKeyDown(KeyCode.P))
                {
                    if (Time.timeScale == 0.0f)
                    {
                        Time.timeScale = _prevTimeScale;
                    }
                    else
                    {
                        _prevTimeScale = Time.timeScale;
                        Time.timeScale = 0.0f;
                    }
                }
            }

#elif ENABLE_INPUT_SYSTEM
            if (!_requireLeftShift || Keyboard.current.leftShiftKey.isPressed)
			{
                if (Keyboard.current.numpad0Key.wasPressedThisFrame) Time.timeScale = 0f;
                if (Keyboard.current.numpad1Key.wasPressedThisFrame) Time.timeScale = 0.01f;
                if (Keyboard.current.numpad2Key.wasPressedThisFrame) Time.timeScale = 0.1f;
                if (Keyboard.current.numpad3Key.wasPressedThisFrame) Time.timeScale = 0.25f;
                if (Keyboard.current.numpad4Key.wasPressedThisFrame) Time.timeScale = 0.5f;
                if (Keyboard.current.numpad5Key.wasPressedThisFrame) Time.timeScale = 1f;
                if (Keyboard.current.numpad6Key.wasPressedThisFrame) Time.timeScale = 2f;
                if (Keyboard.current.numpad7Key.wasPressedThisFrame) Time.timeScale = 5f;
                if (Keyboard.current.numpad8Key.wasPressedThisFrame) Time.timeScale = 10f;
                if (Keyboard.current.numpad9Key.wasPressedThisFrame) Time.timeScale = 25f;

                if (Keyboard.current.pKey.wasPressedThisFrame)
				{
					if (Time.timeScale == 0.0f)
					{
						Time.timeScale = _prevTimeScale;
					}
					else
					{
						_prevTimeScale = Time.timeScale;
						Time.timeScale = 0.0f;
					}
				}
			}
#endif
        }

    }
}
#endif