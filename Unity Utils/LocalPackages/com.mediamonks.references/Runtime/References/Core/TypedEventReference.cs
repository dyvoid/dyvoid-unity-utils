using System;
using System.Diagnostics;

namespace MediaMonks.References
{
	public class TypedEventReference<T> : ScriptableObjectReferenceBase
	{
		public event Action<T> Invoked;

		private uint _counter;
		private string[] _inspectorStats;

		/// <inheritdoc/>
		public override string EditorDescription =>
			$"<b>Description</b>\nRuntime reference to an event with parameter of type <b>{typeof(T)}</b>.";

		/// <inheritdoc/>
		public override bool DrawEditorValueInformation => false;

		/// <inheritdoc/>
		public override string[] EditorInspectorStats => _inspectorStats;

		/// <inheritdoc/>
		public override Type GetTypeOfValue()
		{
			return typeof(Action<T>);
		}

		/// <summary>
		/// Always returns false, <see cref="TypedEventReference"/> does not support setting the value.
		/// </summary>
		public override bool TrySetValue(object value) => false;

		/// <inheritdoc/>
		/// <summary>
		/// Always returns false, <see cref="TypedEventReference"/> does not support getting the value.
		/// </summary>
		public override bool TryGetValue<TOut>(out TOut value)
		{
			value = default;
			return false;
		}

		/// <summary>
		/// Invoke with the specified value.
		/// </summary>
		public void Invoke(T value)
		{
			Invoked?.Invoke(value);

			EditorIncreaseCounter();
			EditorUpdateInspectorStats(value?.ToString());
		}

		protected override void OnFinalize()
		{
			Invoked = null;
			EditorResetCounter();
			_inspectorStats = null;

			EditorUpdateInspectorStats("[NONE]");
		}

		[Conditional("UNITY_EDITOR")]
		private void EditorResetCounter()
		{
			_counter = 0;
		}

		[Conditional("UNITY_EDITOR")]
		private void EditorIncreaseCounter()
		{
			_counter++;
		}

		[Conditional("UNITY_EDITOR")]
		private void EditorUpdateInspectorStats(string parameterValue)
		{
			_inspectorStats = new[]
			{
				"<b>Event Info</b>",
				$"Invokes: {_counter}",
				$"Last value: {parameterValue}"
			};
		}
	}
}