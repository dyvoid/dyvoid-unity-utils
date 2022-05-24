using System;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace MediaMonks.References
{
	/// <summary>
	/// Base class for reference <see cref="ScriptableObject"/>.
	/// </summary>
	public abstract class ScriptableObjectReferenceBase : ScriptableObject
	{
		/// <summary>
		/// The description shown in the Editor Inspector. Override to set to a specific description.
		/// </summary>
		public virtual string EditorDescription => null;

		/// <summary>
		/// Is value information shown in the Editor Inspector? This shows the value in the inspector for value types and a "Select Instance" button for reference types.
		/// Default is true, override to change. E.g. if the value is an event, showing the value content might not make sense.
		/// </summary>
		public virtual bool DrawEditorValueInformation => true;

		/// <summary>
		/// Addition statistics shown in the Editor Inspector. Override to set this information.
		/// </summary>
		public virtual string[] EditorInspectorStats => null;

		/// <summary>
		/// Attempts to set the value by casting from <see cref="System.object"/>.
		/// </summary>
		public abstract bool TrySetValue(object value);

		/// <summary>
		/// Returns the <see cref="Type"/> of the value.
		/// </summary>
		public abstract Type GetTypeOfValue();

		/// <summary>
		/// Attempts to get the value by casting it.
		/// </summary>
		public abstract bool TryGetValue<TOut>(out TOut value);

		/// <summary>
		/// Attempts to get the value by casting it to <see cref="System.object"/> type.
		/// </summary>
		public bool TryGetValue(out object value)
		{
			return TryGetValue<object>(out value);
		}

		/// <summary>
		/// Is called at the play-mode "Start"
		/// </summary>
		protected virtual void Initialize() { }

		/// <summary>
		/// Reset internally used values to ensure they don't get retained between scene and play-mode changes.
		/// </summary>
		protected virtual void OnFinalize() { }

		private void OnEnable()
		{
#if UNITY_EDITOR
			// When starting in editor, OnEnable is called before Application.isPlaying == true.
			// Therefore we have to do this check to run functionality only intended for play mode.
			if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif
			Initialize();
		}

		private void OnDisable()
		{
#if UNITY_EDITOR
			if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif
			OnFinalize();
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(ScriptableObjectReferenceBase), true)]
	public class ReferenceValueBaseEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDescription();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			DrawValueInformation();
			DrawInspectorStats();
		}

		private void DrawDescription()
		{
			if (!(target is ScriptableObjectReferenceBase referenceValueBase)) return;

			if (string.IsNullOrEmpty(referenceValueBase.EditorDescription)) return;

			var style = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
			EditorGUILayout.LabelField(referenceValueBase.EditorDescription, style);
			DrawUILine(Color.gray);
			EditorGUILayout.Space();
		}

		private void DrawInspectorStats()
		{
			if (!(target is ScriptableObjectReferenceBase referenceValueBase)) return;

			string[] info = referenceValueBase.EditorInspectorStats;

			if (info == null) return;

			EditorGUILayout.Space();

			var style = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
			foreach (string infoString in info)
			{
				EditorGUILayout.LabelField(infoString, style);
			}
		}

		private void DrawValueInformation()
		{
			if (!(target is ScriptableObjectReferenceBase referenceValueBase)) return;

			if (!referenceValueBase.DrawEditorValueInformation) return;

			bool wasEnabled = GUI.enabled;
			bool hasValue = referenceValueBase.TryGetValue<object>(out object value);

			EditorGUILayout.Space(2);

			GUI.enabled = Application.isPlaying && hasValue;

			bool isValueType = referenceValueBase.GetTypeOfValue().IsValueType;
			if (isValueType)
			{
				var style = new GUIStyle(GUI.skin.label) { richText = true };
				EditorGUILayout.LabelField($"Current value: <b>{value}</b>", style);
				GUI.enabled = wasEnabled;
				return;
			}

			if (hasValue)
			{
				GUILayout.Label($"Current instance: {value}");
			}

			if (GUILayout.Button("Select Instance"))
			{
				EditorGUIUtility.PingObject(value as Object);
			}

			GUI.enabled = wasEnabled;
		}

		private static void DrawUILine(Color color, int thickness = 1, int padding = 0)
		{
			var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			EditorGUI.DrawRect(r, color);
		}
	}
#endif
}