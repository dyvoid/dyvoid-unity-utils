#if UNITY_EDITOR

using UnityEngine;

namespace Temple.Utils
{
	public class TimeScaler : MonoBehaviour
	{
		private float _prevTimeScale = 1.0f;

        void Update()
		{
            if (Input.anyKeyDown)
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
		}

	}
}
#endif
