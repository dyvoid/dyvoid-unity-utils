using System;
using System.Diagnostics;
using UnityEngine;

namespace MediaMonks.References
{
	[CreateAssetMenu(menuName = "References/Event (no parameters)")]
	public class EventReference : ScriptableObjectReferenceBase
	{
		public event Action Invoked;

		private uint _counter;
		private string[] _inspectorStats;

		public override string EditorDescription =>
			$"<b>Description</b>\nRuntime reference to an event without parameters.";

		public override string[] EditorInspectorStats => _inspectorStats;

		public override bool DrawEditorValueInformation => false;

		public override Type GetTypeOfValue()
		{
			return typeof(Action);
		}

		public override bool TrySetValue(object value) => false;

		public override bool TryGetValue<TOut>(out TOut value)
		{
			value = default;
			return false;
		}

		public void Invoke()
		{
			Invoked?.Invoke();
			EditorIncreaseCounter();
			EditorUpdateInspectorStats();
		}

		protected override void OnFinalize()
		{
			Invoked = null;
			EditorResetCounter();
			_inspectorStats = null;

			EditorUpdateInspectorStats();
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
		private void EditorUpdateInspectorStats()
		{
			_inspectorStats = new[]
			{
				"<b>Event Info</b>",
				$"Invokes: {_counter}"
			};
		}
	}
}