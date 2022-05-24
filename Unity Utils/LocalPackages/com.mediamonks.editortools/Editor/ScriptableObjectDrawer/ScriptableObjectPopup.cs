using UnityEditor;
using UnityEngine;

namespace MediaMonks.EditorTools
{
	public class ScriptableObjectPopup : PopupWindowContent
	{
		private readonly Editor _editor;
		private readonly string _title;
		private Vector2 _scrollPos;

		private readonly GUIStyle _scrollAreaStyle;
		private readonly float _width;

		public ScriptableObjectPopup(ScriptableObject obj, float width)
		{
			_width = width;
			_scrollAreaStyle = new GUIStyle(EditorStyles.helpBox);
			_scrollAreaStyle.margin = new RectOffset(4, 4, 4, 4);
			_scrollAreaStyle.padding = new RectOffset(8, 8, 8, 8);
			_title = obj.name;
			
			Editor.CreateCachedEditor(obj, null, ref _editor);
		}

		public override Vector2 GetWindowSize()
		{
			return new Vector2(_width, 300);
		}

		public override void OnGUI(Rect rect)
		{
			EditorGUILayout.LabelField(_title, EditorStyles.miniLabel);
			EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));
			using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos, _scrollAreaStyle))
			{
				_editor.OnInspectorGUI();
				_scrollPos = scroll.scrollPosition;
			}
		}
	}
}
