using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MediaMonks.EditorTools
{
	[CustomPropertyDrawer(typeof(ScriptableObject), true)]
	public class ScriptableObjectPropertyDrawer : PropertyDrawer
	{
		private const string _lastSelectedFolderKey = "create_so_asset_folder";

		private readonly Lazy<GUIContent> _addButtonContent = new Lazy<GUIContent>(() => new GUIContent("+", "Create a new ScriptableObject asset"));
		private readonly Lazy<GUIContent> _magicButtonContent = new Lazy<GUIContent>(() => new GUIContent("!", "Try to guess the most applicable ScriptableObject of the correct type"));

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.PropertyScope(position, GUIContent.none, property))
			{
				const float buttonWidth = 42;
				var contentPosition = EditorGUI.PrefixLabel(position, label);

				var fieldPosition = contentPosition;
				fieldPosition.xMax = contentPosition.xMax - buttonWidth;

				var buttonPosition = contentPosition;
				buttonPosition.xMin = contentPosition.xMax - buttonWidth;

				EditorGUI.PropertyField(fieldPosition, property, GUIContent.none, true);

				if (property.objectReferenceValue != null && GUI.Button(buttonPosition, " ", EditorStyles.popup))
				{
					var popup = new ScriptableObjectPopup(property.objectReferenceValue as ScriptableObject, EditorGUIUtility.currentViewWidth);
					var popupPosition = position;
					popupPosition.x -= buttonWidth;

					PopupWindow.Show(popupPosition, popup);
				}
				else if (property.objectReferenceValue == null)
				{
					DrawCreateGUI(property, buttonPosition);
				}
			}
		}

		private void DrawCreateGUI(SerializedProperty property, Rect buttonPosition)
		{
			var addPosition = buttonPosition;
			addPosition.width = buttonPosition.width / 2;

			if (GUI.Button(addPosition, _addButtonContent.Value))
			{
				var typeCandidates = GetTypeCandidates(fieldInfo);

				if (typeCandidates.Length == 1)
				{
					CreateAsset(property, typeCandidates[0]);
				}
				else if (typeCandidates.Length > 0)
				{
					var menu = new GenericMenu();
					foreach (var type in typeCandidates)
					{
						menu.AddItem(new GUIContent($"Create {type}"), false, x => CreateAsset(property, x as Type), type);
					}

					menu.ShowAsContext();
				}
			}

			var magicPosition = buttonPosition;
			magicPosition.xMin += addPosition.width;

			if (GUI.Button(magicPosition, _magicButtonContent.Value))
			{
				var typeCandidates = GetTypeCandidates(fieldInfo);

				// apply some heuristics to guess which object is the most likely to be needed
				// (i.e. the most changed, latest updated one)
				var asset = typeCandidates.SelectMany(t => LoadAssetsByType(t))
				                          .OrderByDescending(x => EditorUtility.GetDirtyCount(x.asset))
				                          .ThenByDescending(x => File.GetLastAccessTime(x.path))
				                          .Select(x => x.asset)
				                          .FirstOrDefault();

				if (asset != null)
				{
					property.serializedObject.Update();
					property.objectReferenceValue = asset;
					property.serializedObject.ApplyModifiedProperties();
				}
				else
				{
					EditorWindow.focusedWindow.ShowNotification(new GUIContent($"Could not find\n asset of type\n'{typeCandidates}'"));
				}
			}
		}

		private static void CreateAsset(SerializedProperty property, Type type)
		{
			var folderName = SessionState.GetString(_lastSelectedFolderKey, Application.dataPath);
			var assetPath = EditorUtility.SaveFilePanelInProject($"Create new {type}", $"{type}.asset", "asset", "", folderName);
			if (string.IsNullOrWhiteSpace(assetPath)) return;

			SessionState.SetString(_lastSelectedFolderKey, Path.GetDirectoryName(assetPath));

			var newObject = ScriptableObject.CreateInstance(type);

			AssetDatabase.CreateAsset(newObject, assetPath);
			AssetDatabase.SaveAssets();

			property.serializedObject.Update();
			property.objectReferenceValue = newObject;
			property.serializedObject.ApplyModifiedProperties();
		}

		private static Type[] GetTypeCandidates(FieldInfo fieldInfo)
		{
			var actualType = fieldInfo.FieldType;

			if (fieldInfo.FieldType.IsArray)
			{
				actualType = fieldInfo.FieldType.GetElementType();
			}
			else if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
			{
				// this is a list so assume the first generic type argument
				actualType = fieldInfo.FieldType.GenericTypeArguments.FirstOrDefault();
			}

			return actualType?.GenericTypeArguments.Length > 0 
				? TypeCache.GetTypesDerivedFrom(actualType).ToArray() 
				: new[] { actualType };
		}

		private static IEnumerable<(string path, UnityEngine.Object asset)> LoadAssetsByType(Type type)
		{
			return AssetDatabase.FindAssets($"t:{type}")
			                    .Select(AssetDatabase.GUIDToAssetPath)
			                    .Select(path => (path, asset: AssetDatabase.LoadAssetAtPath(path, type)));
		}
	}
}