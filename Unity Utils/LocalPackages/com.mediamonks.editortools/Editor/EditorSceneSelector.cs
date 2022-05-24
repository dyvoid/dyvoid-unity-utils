using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MediaMonks.EditorTools
{
	public class EditorSceneSelector : EditorWindow
	{
		[SerializeField] private int _selectedSceneIndex;
		[SerializeField] private bool _isVerticalLayout;

		/// <summary>
		/// Names displayed in the dropdown list
		/// </summary>
		private string[] _displayedSceneNames = { };

		/// <summary>
		/// Dictionary connecting the list index to the actual scene path
		/// </summary>
		private readonly Dictionary<int, string> _scenesPaths = new Dictionary<int, string>();

		private DateTime _lastListUpdateTime;

		protected virtual string GetTitle() => "Scene Selector";
		protected virtual string GetSceneListTitle() => "Scenes in Build Settings";

		private void Awake()
		{
			UpdateScenesList();
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged += EditorApplicationOnPlaybackStateChange;
			EditorBuildSettings.sceneListChanged += EditorBuildSettingsOnSceneListChanged;
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= EditorApplicationOnPlaybackStateChange;
			EditorBuildSettings.sceneListChanged -= EditorBuildSettingsOnSceneListChanged;
		}

		private void OnGUI()
		{
			titleContent.text = GetTitle();

			GUI.enabled = !IsEditorChangeInProgress();

			DrawSelectionBar();
			EditorGUILayout.Space();
			DrawButtons();
		}

		private static bool IsEditorChangeInProgress()
		{
			return EditorApplication.isPlayingOrWillChangePlaymode
			       || EditorApplication.isUpdating
			       || EditorApplication.isCompiling;
		}

		private void DrawSelectionBar()
		{
			GUILayout.Label(GetSceneListTitle());

			if (_selectedSceneIndex >= _displayedSceneNames.Length)
			{
				_selectedSceneIndex = 0;
			}

			EditorGUILayout.BeginHorizontal();

			_selectedSceneIndex =
				EditorGUILayout.Popup(_selectedSceneIndex, _displayedSceneNames.Length > 0 ? _displayedSceneNames : new[] { "" });

			var mousePosition = Event.current.mousePosition;

			// Update list if mouse over popup and hasn't been updated for 10 seconds
			bool shouldUpdateList = GUILayoutUtility.GetLastRect().Contains(mousePosition)
			                        && (DateTime.Now - _lastListUpdateTime).Seconds > 10;
			if (shouldUpdateList) UpdateScenesList();

			bool toggleClicked = GUILayout.Button(
				new GUIContent(_isVerticalLayout ? "V" : "H", "Toggle between Horizontal and Vertical layout."),
				GUILayout.Height(20), GUILayout.Width(20));
			if (toggleClicked) _isVerticalLayout = !_isVerticalLayout;

			bool selectClicked = GUILayout.Button(new GUIContent("S", "Select scene in the project."), GUILayout.Height(20),
				GUILayout.Width(20));
			if (selectClicked) SelectSceneFromList(_selectedSceneIndex);

			EditorGUILayout.EndHorizontal();
		}

		private void DrawButtons()
		{
			if (!_isVerticalLayout) EditorGUILayout.BeginHorizontal();

			int buttonHeight = _isVerticalLayout ? 30 : 22;

			bool openClicked = GUILayout.Button("Open Selected", GUILayout.Height(buttonHeight));
			if (openClicked) OpenSceneFromList(_selectedSceneIndex);

			bool playSelectedClicked = GUILayout.Button("Play Selected", GUILayout.Height(buttonHeight));
			if (playSelectedClicked) StartSceneFromList(_selectedSceneIndex);

			bool playFirstClicked = GUILayout.Button("Play First", GUILayout.Height(buttonHeight));
			if (playFirstClicked) StartSceneFromList(0);

			if (!_isVerticalLayout) EditorGUILayout.EndHorizontal();
		}

		private void EditorBuildSettingsOnSceneListChanged()
		{
			UpdateScenesList();
		}

		private void UpdateScenesList()
		{
			var sceneNamesAndPaths = GetSceneNamesAndPaths().Distinct().ToList();

			_scenesPaths.Clear();
			foreach (var namesAndPath in sceneNamesAndPaths)
			{
				_scenesPaths.Add(_scenesPaths.Count, namesAndPath.Item2);
			}

			_displayedSceneNames = sceneNamesAndPaths.Select(namesAndPath => namesAndPath.Item1).ToArray();

			if (_selectedSceneIndex >= _displayedSceneNames.Length)
			{
				_selectedSceneIndex = 0;
			}

			_lastListUpdateTime = DateTime.Now;
		}

		protected virtual IEnumerable<(string, string)> GetSceneNamesAndPaths()
		{
			var sceneNamesAndPaths = new List<(string, string)>();

			var editorBuildSettingsScenes = EditorBuildSettings.scenes;

			foreach (var scene in editorBuildSettingsScenes)
			{
				if (!IncludeSceneInList(scene.path)) continue;

				string scenePath = scene.path;
				string sceneName = Path.GetFileNameWithoutExtension(scenePath);

				// Add asterisk to indicate scene is not included in build
				if (!scene.enabled) sceneName = $"* {sceneName}";

				sceneNamesAndPaths.Add((sceneName, scenePath));
			}

			return sceneNamesAndPaths;
		}

		protected virtual bool IncludeSceneInList(string scenePath)
		{
			// Add project specific scene filtering here
			return true;
		}

		private void SelectSceneFromList(int index)
		{
			UpdateScenesList();

			var selectedScene = AssetDatabase.LoadMainAssetAtPath(_scenesPaths[index]);
			if (!selectedScene) return;

			Selection.activeObject = selectedScene;
			EditorGUIUtility.PingObject(selectedScene);
		}

		// ============================================================================
		// Scene open and play handling
		// ============================================================================

		private static bool _playPressed;

		private const string LastSceneNameKey = "EditorSceneSelector.lastscene";
		private const string EnteredPlayKey = "EditorSceneSelector.enteredPlay";

		private void EditorApplicationOnPlaybackStateChange(PlayModeStateChange state)
		{
			UpdateScenesList();

			if (_playPressed) return;
			if (!SessionState.GetBool(EnteredPlayKey, false)) return;
			if (EditorApplication.isPlaying) return;

			SessionState.EraseBool(EnteredPlayKey);

			string scenePath = SessionState.GetString(LastSceneNameKey, "");
			SessionState.EraseString(LastSceneNameKey);

			if (!string.IsNullOrEmpty(scenePath))
			{
				EditorSceneManager.OpenScene(scenePath);
			}
		}

		private void StartSceneFromList(int index)
		{
			UpdateScenesList();

			_playPressed = true;
			string currentScene = SceneManager.GetActiveScene().path;

			if (!string.IsNullOrEmpty(currentScene))
			{
				SessionState.SetString(LastSceneNameKey, currentScene);
			}

			SessionState.SetBool(EnteredPlayKey, true);
			OpenScene(_scenesPaths[index], true);
		}

		private void OpenSceneFromList(int index)
		{
			UpdateScenesList();
			OpenScene(_scenesPaths[index], false);
		}

		private static void OpenScene(string scene, bool run)
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				return;

			EditorSceneManager.OpenScene(scene);
			EditorApplication.isPlaying = run;
		}

		// ============================================================================
		// Menu item handling
		// ============================================================================

		[MenuItem("Tools/MediaMonks/Open Scene Selector...")]
		private static void OpenSceneSelector()
		{
			// Get existing open window or if none, make a new one:
			GetWindow(typeof(EditorSceneSelector));
		}
	}
}