using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MediaMonks.EditorTools
{
	/// <summary>
	/// A component to add an editor-only description to a GameObject.
	/// </summary>
	[AddComponentMenu("Tools/Description")]
	public class Description : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] private string _text;
		[SerializeField] private Color _lineColor = new Color32(0x00, 0x99, 0x00, 0xff);

		public string Text
		{
			get => _text;
			set => _text = value;
		}

		public Color LineColor
		{
			get => _lineColor;
			set => _lineColor = value;
		}
#endif
	}

#if UNITY_EDITOR
	public static class DescriptionMenu
	{
		[MenuItem("Tools/Descriptions/Show Descriptions in hierarchy")]
		public static void FilterHierarchy()
		{
			SceneModeUtility.SearchForType(typeof(Description));
		}
	}

	[CustomEditor(typeof(Description), false)]
	public class DocumentationEditor : Editor
	{
		private bool EditText { get; set; }

		private Description Description => (Description)target;

		public override void OnInspectorGUI()
		{
			GUILayout.Space(5);

			if (Application.isPlaying)
			{
				EditText = false;
			}
			else if (string.IsNullOrWhiteSpace(Description.Text))
			{
				EditText = true;
			}

			if (EditText)
			{
				EditDescription(Description);
			}
			else
			{
				ShowDescription(Description);
			}

			if (Application.isPlaying) return;


			// Detect double click to start editing.
			if (Event.current.clickCount == 2 && !EditText)
			{
				EditText = true;
			}

			if (!EditText) return;

			GUILayout.Space(5);
			EditText = !GUILayout.Button("Done", GUILayout.Width(50));
		}

		private static void ShowDescription(Description documentation)
		{
			HorizontalLine(documentation.LineColor, 2, Vector2.zero);
			GUILayout.Space(5);

			var textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;

			var style = new GUIStyle
			{
				richText = true,
				normal = new GUIStyleState { textColor = textColor },
				wordWrap = true,
			};

			GUILayout.Label(new GUIContent(documentation.Text, "Double click to edit the description."), style);

			GUILayout.Space(5);
			HorizontalLine(documentation.LineColor, 2, Vector2.zero);
		}

		private void EditDescription(Description description)
		{
			float viewWidth = GetViewWidth();

			description.LineColor = EditorGUILayout.ColorField("Line Color", description.LineColor);
			GUILayout.Space(5);

			var style = GUI.skin.textArea;
			style.richText = false;
			style.wordWrap = true;

			float boxHeight = Mathf.Max(100, style.CalcHeight(new GUIContent(description.Text), viewWidth));
			Description.Text = GUILayout.TextArea(description.Text, style, GUILayout.MinHeight(boxHeight), GUILayout.Width(viewWidth));

			Undo.RecordObject(Description, "Description change.");
			PrefabUtility.RecordPrefabInstancePropertyModifications(Description);
		}

		private static void HorizontalLine(Color color, float height, Vector2 margin)
		{
			GUILayout.Space(margin.x);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), color);
			GUILayout.Space(margin.y);
		}

		private Rect _widthRect;

		/// <summary>
		/// hacky way to get the actual width of the view. 
		/// </summary>
		private float GetViewWidth()
		{
			GUILayout.Label("hack", GUILayout.MaxHeight(0));
			if (Event.current.type == EventType.Repaint)
			{
				// hack to get real view width
				_widthRect = GUILayoutUtility.GetLastRect();
			}

			return _widthRect.width;
		}
	}
#endif
}