using UnityEditor;
using UnityEngine;

namespace MediaMonks.EditorTools
{
// #if UNITY_2018 || UNITY_2019
	/// <summary>
	/// Pinned editor windows keep the editor for an asset open, persisting trough assembly reloads and
	/// usually even editor restarts. Very useful if there is a settings object or prefab that you edit
	/// or refer to frequently.
	///
	/// Note that in Unity 2020+ the confusingly named "properties" (Assets/Properties...) window was added which does more or
	/// less the same thing.
	/// </summary>
	public class PinnedEditorWindow : EditorWindow
	{
		private const string _menuItem = "Tools/MediaMonks/Pinned Editor/";

		[SerializeField] private Object _pinnedObject;

		private Editor _editor;
		private GUIStyle _style;
		private Vector2 _scrollPosition;

		[MenuItem(_menuItem + "Open Dockable Pinned Editor...")]
		private static void OpenDockableFromMenu() => Create(false);

		[MenuItem(_menuItem + "Open Front Pinned Editor...")]
		private static void OpenFrontFromMenu() => Create(true);

		private static PinnedEditorWindow Create(bool keepInFront)
		{
			var window = CreateInstance<PinnedEditorWindow>();

			if (keepInFront)
			{
				window.ShowUtility();
			}
			else
			{
				window.Show();
			}

			return window;
		}

		[MenuItem("Assets/Open in Dockable Pinned Editor...")]
		private static void CreateFromSelection()
		{
			var window = Create(false);
			window._pinnedObject = Selection.activeObject;
			window.UpdateWindowTitle();
		}

		[MenuItem("Assets/Open in Front Pinned Editor...")]
		private static void CreateFromSelectionInFront()
		{
			var window = Create(true);
			window._pinnedObject = Selection.activeObject;
			window.UpdateWindowTitle();
		}

		private void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			UpdateWindowTitle();
		}

		private void OnGUI()
		{
			if (_style == null)
			{
				_style = new GUIStyle(EditorStyles.helpBox)
				{
					margin = new RectOffset(16, 16, 16, 4),
					padding = new RectOffset(8, 8, 8, 8),
				};
			}

			ObjectPickerGUI();
			PinnedObjectGUI();
		}

		private void ObjectPickerGUI()
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.Separator();
				_pinnedObject = EditorGUILayout.ObjectField("Pinned Object", _pinnedObject, typeof(Object), false);

				if (check.changed)
				{
					UpdateWindowTitle();
					_editor = null;
				}
			}
		}

		private void PinnedObjectGUI()
		{
			if (_pinnedObject == null) return;

			Editor.CreateCachedEditor(_pinnedObject, null, ref _editor);
			if (_editor != null)
			{
				using (var s = new EditorGUILayout.ScrollViewScope(_scrollPosition))
				using (new EditorGUILayout.VerticalScope(_style))
				{
					_editor.OnInspectorGUI();
					_scrollPosition = s.scrollPosition;
				}
			}
		}

		private void UpdateWindowTitle()
		{
			titleContent = _pinnedObject == null
				? new GUIContent("★ (None)")
				: new GUIContent($"★ {_pinnedObject.name}");
		}
	}
// #endif
}