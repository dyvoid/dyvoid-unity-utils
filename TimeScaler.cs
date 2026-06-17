#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace dyvoid.Utility
{
	/// <summary>
	/// A utility class to manage and manipulate the time scale in Unity by pressing 0-9.
	/// Press '=' / '-' to step through scales, 'p' for pause.
	/// </summary>
	public class TimeScaler : MonoBehaviour
	{
		private const int DefaultScaleIndex = 5;

		private static readonly float[] TimeScales =
		{
			0.0f, 0.01f, 0.1f, 0.25f, 0.5f, 1.0f, 2.0f, 5.0f, 10.0f, 25.0f
		};

		[Header("Keys")]
		[SerializeField] private bool requireLeftShift = true;
		[SerializeField] private NumberKeys numberKeys = NumberKeys.Numpad;
		[SerializeField] private PauseKey pauseKey = PauseKey.P;

		[Header("Overlay")]
		[SerializeField] private bool showOverlay = true;
		[SerializeField] private float overlayDuration = 1.5f;
		[SerializeField] private float overlayFadeDuration = 0.5f;

		private float prevTimeScale = 1.0f;
		private bool isPaused;
		private int currentScaleIndex = DefaultScaleIndex;
		private Cooldown overlayCooldown;

		private void Awake()
		{
			overlayCooldown = new Cooldown(overlayDuration);
			overlayCooldown.Reset(0f);
		}

		private void OnDisable()
		{
			Time.timeScale = 1.0f;
			isPaused = false;
		}

		private void Update()
		{
			overlayCooldown.UpdateTime(Time.unscaledDeltaTime);

			if (!IsInputAllowed()) return;

			if (TryGetPressedDigit(out int digit))
			{
				currentScaleIndex = digit;
				SetTimeScale(TimeScales[currentScaleIndex]);
			}

			if (TryGetStep(out int step))
			{
				currentScaleIndex = Mathf.Clamp(currentScaleIndex + step, 0, TimeScales.Length - 1);
				SetTimeScale(TimeScales[currentScaleIndex]);
			}

			if (IsPausePressed())
			{
				TogglePause();
			}
		}

		private void OnGUI()
		{
			if (!showOverlay || overlayCooldown.HasExpired) return;

			float alpha = overlayFadeDuration > 0f
				? Mathf.Clamp01(overlayCooldown.TimeLeft / overlayFadeDuration)
				: 1f;

			Color baseColor = isPaused ? Color.yellow : Color.white;
			GUI.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
			string label = isPaused ? "PAUSED" : $"{Time.timeScale:0.##}x";
			GUI.Label(new Rect(10, 10, 120, 22), $"Time: {label}");
			GUI.color = Color.white;
		}

		/// <summary>
		/// Returns whether input should be processed this frame, based on the modifier requirement.
		/// </summary>
		private bool IsInputAllowed()
		{
			if (!requireLeftShift) return true;
			return Keyboard.current.leftShiftKey.isPressed;
		}

		/// <summary>
		/// Checks if any digit key (0-9) was pressed this frame.
		/// </summary>
		private bool TryGetPressedDigit(out int digit)
		{
			bool checkTopRow = numberKeys != NumberKeys.Numpad;
			bool checkNumpad = numberKeys != NumberKeys.TopRow;

			var kb = Keyboard.current;
			if (kb == null) { digit = -1; return false; }
			for (int i = 0; i <= 9; i++)
			{
				Key topRowKey = (i == 0) ? Key.Digit0 : (Key.Digit1 + (i - 1));
				if ((checkTopRow && kb[topRowKey].wasPressedThisFrame) ||
				    (checkNumpad && kb[Key.Numpad0 + i].wasPressedThisFrame))
				{
					digit = i;
					return true;
				}
			}

			digit = -1;
			return false;
		}

		private bool IsPausePressed()
		{
			if (pauseKey == PauseKey.None) return false;

			var kb = Keyboard.current;
			if (kb == null) return false;
			KeyControl key = pauseKey switch
			{
				PauseKey.P => kb.pKey,
				PauseKey.Escape => kb.escapeKey,
				PauseKey.Space => kb.spaceKey,
				PauseKey.Pause => kb.pauseKey,
				_ => null
			};
			return key?.wasPressedThisFrame ?? false;
		}

		/// <summary>
		/// Checks if a step key was pressed this frame ('=' or '+' for up, '-' for down).
		/// </summary>
		private bool TryGetStep(out int step)
		{
			var kb = Keyboard.current;
			if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame)
			{
				step = 1;
				return true;
			}
			if (kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame)
			{
				step = -1;
				return true;
			}

			step = 0;
			return false;
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

			overlayCooldown.Reset();
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

			overlayCooldown.Reset();
		}

		public enum NumberKeys
		{
			Numpad,
			TopRow,
			Both
		}

		public enum PauseKey
		{
			None,
			P,
			Escape,
			Space,
			Pause
		}
	}
}
#endif
