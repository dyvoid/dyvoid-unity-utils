using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace State.Editor
{
	/// <summary>
	/// Editor wizard for generating FSM boilerplate code.
	/// Creates state enum, FSM class, state behaviours, and optionally blackboard/visibility classes.
	/// </summary>
	public class FSMWizard : EditorWindow
	{
		private string fsmName = "Player";
		private string namespaceName = "";
		private List<string> states = new List<string> { "Idle" };
		private bool generateBlackboard = true;
		private bool generateVisibility = false;
		private bool singleFileForStates = false;
		private string outputFolder = "Assets/Scripts";

		private Vector2 statesScrollPosition;

		[MenuItem("Tools/FSM/Create FSM...")]
		public static void ShowWindow()
		{
			var window = GetWindow<FSMWizard>("Create FSM");
			window.minSize = new Vector2(400, 500);
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("Create Finite State Machine", EditorStyles.boldLabel);
			EditorGUILayout.Space(10);

			// Name and namespace.
			fsmName = EditorGUILayout.TextField("Name", fsmName);
			namespaceName = EditorGUILayout.TextField("Namespace (optional)", namespaceName);

			EditorGUILayout.Space(10);

			// States list.
			EditorGUILayout.LabelField("States", EditorStyles.boldLabel);

			statesScrollPosition = EditorGUILayout.BeginScrollView(statesScrollPosition, GUILayout.Height(150));
			for (int i = 0; i < states.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				states[i] = EditorGUILayout.TextField(states[i]);
				if (GUILayout.Button("X", GUILayout.Width(25)))
				{
					states.RemoveAt(i);
					i--;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("+ Add State"))
			{
				states.Add("NewState");
			}

			EditorGUILayout.Space(10);

			// Options.
			generateBlackboard = EditorGUILayout.Toggle("Generate Blackboard", generateBlackboard);
			generateVisibility = EditorGUILayout.Toggle("Generate Visibility Controller", generateVisibility);

			EditorGUILayout.Space(5);

			singleFileForStates = EditorGUILayout.Toggle("Single File for All States", singleFileForStates);

			EditorGUILayout.Space(10);

			// Output folder.
			EditorGUILayout.BeginHorizontal();
			outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
			if (GUILayout.Button("...", GUILayout.Width(30)))
			{
				string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
				if (!string.IsNullOrEmpty(selectedPath))
				{
					// Convert absolute path to relative.
					if (selectedPath.StartsWith(Application.dataPath))
					{
						outputFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
					}
					else
					{
						outputFolder = selectedPath;
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(20);

			// Validation.
			bool isValid = ValidateInput(out string validationError);
			if (!isValid)
			{
				EditorGUILayout.HelpBox(validationError, MessageType.Warning);
			}

			EditorGUI.BeginDisabledGroup(!isValid);
			if (GUILayout.Button("Generate", GUILayout.Height(30)))
			{
				Generate();
			}
			EditorGUI.EndDisabledGroup();
		}

		private bool ValidateInput(out string error)
		{
			error = "";

			if (string.IsNullOrWhiteSpace(fsmName))
			{
				error = "Name is required.";
				return false;
			}

			if (!IsValidIdentifier(fsmName))
			{
				error = "Name must be a valid C# identifier.";
				return false;
			}

			if (!string.IsNullOrWhiteSpace(namespaceName) && !IsValidNamespace(namespaceName))
			{
				error = "Namespace must be a valid C# namespace.";
				return false;
			}

			if (states.Count == 0)
			{
				error = "At least one state is required.";
				return false;
			}

			foreach (string state in states)
			{
				if (string.IsNullOrWhiteSpace(state))
				{
					error = "State names cannot be empty.";
					return false;
				}

				if (!IsValidIdentifier(state))
				{
					error = $"State '{state}' is not a valid C# identifier.";
					return false;
				}
			}

			// Check for duplicate states.
			var stateSet = new HashSet<string>();
			foreach (string state in states)
			{
				if (!stateSet.Add(state))
				{
					error = $"Duplicate state: '{state}'.";
					return false;
				}
			}

			return true;
		}

		private bool IsValidIdentifier(string name)
		{
			if (string.IsNullOrEmpty(name)) return false;
			if (!char.IsLetter(name[0]) && name[0] != '_') return false;

			foreach (char c in name)
			{
				if (!char.IsLetterOrDigit(c) && c != '_') return false;
			}

			return true;
		}

		private bool IsValidNamespace(string ns)
		{
			string[] parts = ns.Split('.');
			foreach (string part in parts)
			{
				if (!IsValidIdentifier(part)) return false;
			}

			return true;
		}

		private void Generate()
		{
			// Ensure output folder exists.
			string fullOutputPath = Path.Combine(Application.dataPath, "..", outputFolder);
			fullOutputPath = Path.GetFullPath(fullOutputPath);

			if (!Directory.Exists(fullOutputPath))
			{
				Directory.CreateDirectory(fullOutputPath);
			}

			var filesToGenerate = new List<(string path, string content, string description)>();

			// Collect all files to generate.
			string enumPath = Path.Combine(fullOutputPath, $"{fsmName}State.cs");
			filesToGenerate.Add((enumPath, GenerateEnum(), "State Enum"));

			string fsmPath = Path.Combine(fullOutputPath, $"{fsmName}FSM.cs");
			filesToGenerate.Add((fsmPath, GenerateFSMClass(), "FSM Class"));

			if (generateBlackboard)
			{
				string blackboardPath = Path.Combine(fullOutputPath, $"{fsmName}Blackboard.cs");
				filesToGenerate.Add((blackboardPath, GenerateBlackboard(), "Blackboard"));
			}

			if (generateVisibility)
			{
				string visibilityPath = Path.Combine(fullOutputPath, $"{fsmName}StateVisibility.cs");
				filesToGenerate.Add((visibilityPath, GenerateVisibility(), "Visibility Controller"));
			}

			if (singleFileForStates)
			{
				string statesPath = Path.Combine(fullOutputPath, $"{fsmName}StateBehaviours.cs");
				filesToGenerate.Add((statesPath, GenerateAllStateBehaviours(), "State Behaviours"));
			}
			else
			{
				foreach (string state in states)
				{
					string statePath = Path.Combine(fullOutputPath, $"{fsmName}{state}StateBehaviour.cs");
					filesToGenerate.Add((statePath, GenerateStateBehaviour(state), $"{state} State Behaviour"));
				}
			}

			// Check for existing files.
			var existingFiles = new List<string>();
			foreach (var (path, _, description) in filesToGenerate)
			{
				if (File.Exists(path))
				{
					existingFiles.Add($"{description} ({Path.GetFileName(path)})");
				}
			}

			if (existingFiles.Count > 0)
			{
				string message = "The following files already exist:\n\n" +
					string.Join("\n", existingFiles) +
					"\n\nWhat would you like to do?";

				int result = EditorUtility.DisplayDialogComplex(
					"Files Already Exist",
					message,
					"Overwrite All",
					"Cancel",
					"Skip Existing"
				);

				if (result == 1)
				{
					// Cancel.
					return;
				}

				bool skipExisting = result == 2;

				// Write files.
				int written = 0;
				int skipped = 0;

				foreach (var (path, content, description) in filesToGenerate)
				{
					if (skipExisting && File.Exists(path))
					{
						skipped++;
						continue;
					}

					File.WriteAllText(path, content);
					written++;
				}

				AssetDatabase.Refresh();

				string resultMessage = $"Generated {written} file(s).";
				if (skipped > 0)
				{
					resultMessage += $" Skipped {skipped} existing file(s).";
				}

				EditorUtility.DisplayDialog("FSM Generated", resultMessage, "OK");
			}
			else
			{
				// No conflicts, write all files.
				foreach (var (path, content, _) in filesToGenerate)
				{
					File.WriteAllText(path, content);
				}

				AssetDatabase.Refresh();
				EditorUtility.DisplayDialog("FSM Generated", $"Successfully generated {filesToGenerate.Count} file(s).", "OK");
			}
		}

		private string GenerateEnum()
		{
			var sb = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(namespaceName))
			{
				sb.AppendLine($"namespace {namespaceName}");
				sb.AppendLine("{");
				sb.AppendLine($"\tpublic enum {fsmName}State");
				sb.AppendLine("\t{");

				foreach (string state in states)
				{
					sb.AppendLine($"\t\t{state},");
				}

				sb.AppendLine("\t}");
				sb.AppendLine("}");
			}
			else
			{
				sb.AppendLine($"public enum {fsmName}State");
				sb.AppendLine("{");

				foreach (string state in states)
				{
					sb.AppendLine($"\t{state},");
				}

				sb.AppendLine("}");
			}

			return sb.ToString();
		}

		private string GenerateFSMClass()
		{
			var sb = new StringBuilder();
			sb.AppendLine("using State;");
			sb.AppendLine();

			if (!string.IsNullOrWhiteSpace(namespaceName))
			{
				sb.AppendLine($"namespace {namespaceName}");
				sb.AppendLine("{");
				sb.AppendLine($"\tpublic class {fsmName}FSM : FSM<{fsmName}State>");
				sb.AppendLine("\t{");
				sb.AppendLine("\t}");
				sb.AppendLine("}");
			}
			else
			{
				sb.AppendLine($"public class {fsmName}FSM : FSM<{fsmName}State>");
				sb.AppendLine("{");
				sb.AppendLine("}");
			}

			return sb.ToString();
		}

		private string GenerateBlackboard()
		{
			var sb = new StringBuilder();
			sb.AppendLine("using State;");
			sb.AppendLine();

			if (!string.IsNullOrWhiteSpace(namespaceName))
			{
				sb.AppendLine($"namespace {namespaceName}");
				sb.AppendLine("{");
				sb.AppendLine($"\tpublic class {fsmName}Blackboard : FSMBlackboard");
				sb.AppendLine("\t{");
				sb.AppendLine("\t\t// Add shared references and data here.");
				sb.AppendLine("\t}");
				sb.AppendLine("}");
			}
			else
			{
				sb.AppendLine($"public class {fsmName}Blackboard : FSMBlackboard");
				sb.AppendLine("{");
				sb.AppendLine("\t// Add shared references and data here.");
				sb.AppendLine("}");
			}

			return sb.ToString();
		}

		private string GenerateVisibility()
		{
			var sb = new StringBuilder();
			sb.AppendLine("using State;");
			sb.AppendLine("using Utility;");
			sb.AppendLine("using UnityEngine;");
			sb.AppendLine();

			if (!string.IsNullOrWhiteSpace(namespaceName))
			{
				sb.AppendLine($"namespace {namespaceName}");
				sb.AppendLine("{");
				sb.AppendLine($"\tpublic class {fsmName}StateVisibility : FSMStateVisibility<{fsmName}State>");
				sb.AppendLine("\t{");
				sb.AppendLine($"\t\t[SerializeField] private {fsmName}FSM fsm;");
				sb.AppendLine();
				sb.AppendLine($"\t\tprotected override ObservableValue<{fsmName}State> GetStateObservable()");
				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t\treturn fsm.StateObservable;");
				sb.AppendLine("\t\t}");
				sb.AppendLine("\t}");
				sb.AppendLine("}");
			}
			else
			{
				sb.AppendLine($"public class {fsmName}StateVisibility : FSMStateVisibility<{fsmName}State>");
				sb.AppendLine("{");
				sb.AppendLine($"\t[SerializeField] private {fsmName}FSM fsm;");
				sb.AppendLine();
				sb.AppendLine($"\tprotected override ObservableValue<{fsmName}State> GetStateObservable()");
				sb.AppendLine("\t{");
				sb.AppendLine("\t\treturn fsm.StateObservable;");
				sb.AppendLine("\t}");
				sb.AppendLine("}");
			}

			return sb.ToString();
		}

		private string GenerateStateBehaviour(string state)
		{
			var sb = new StringBuilder();
			sb.AppendLine("using State;");
			sb.AppendLine();

			string baseClass = generateBlackboard
				? $"FSMStateBehaviour<{fsmName}State, {fsmName}Blackboard>"
				: $"FSMStateBehaviour<{fsmName}State>";

			if (!string.IsNullOrWhiteSpace(namespaceName))
			{
				sb.AppendLine($"namespace {namespaceName}");
				sb.AppendLine("{");
				sb.AppendLine($"\tpublic class {fsmName}{state}StateBehaviour : {baseClass}");
				sb.AppendLine("\t{");
				sb.AppendLine($"\t\tpublic override {fsmName}State State => {fsmName}State.{state};");
				sb.AppendLine();
				sb.AppendLine($"\t\tpublic override void OnEnter({fsmName}State prevState)");
				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t}");
				sb.AppendLine();
				sb.AppendLine($"\t\tpublic override void OnExit({fsmName}State nextState)");
				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t}");
				sb.AppendLine();
				sb.AppendLine("\t\tpublic override void OnUpdateState()");
				sb.AppendLine("\t\t{");
				sb.AppendLine("\t\t}");
				sb.AppendLine("\t}");
				sb.AppendLine("}");
			}
			else
			{
				sb.AppendLine($"public class {fsmName}{state}StateBehaviour : {baseClass}");
				sb.AppendLine("{");
				sb.AppendLine($"\tpublic override {fsmName}State State => {fsmName}State.{state};");
				sb.AppendLine();
				sb.AppendLine($"\tpublic override void OnEnter({fsmName}State prevState)");
				sb.AppendLine("\t{");
				sb.AppendLine("\t}");
				sb.AppendLine();
				sb.AppendLine($"\tpublic override void OnExit({fsmName}State nextState)");
				sb.AppendLine("\t{");
				sb.AppendLine("\t}");
				sb.AppendLine();
				sb.AppendLine("\tpublic override void OnUpdateState()");
				sb.AppendLine("\t{");
				sb.AppendLine("\t}");
				sb.AppendLine("}");
			}

			return sb.ToString();
		}

		private string GenerateAllStateBehaviours()
		{
			var sb = new StringBuilder();
			sb.AppendLine("using State;");
			sb.AppendLine();

			string baseClass = generateBlackboard
				? $"FSMStateBehaviour<{fsmName}State, {fsmName}Blackboard>"
				: $"FSMStateBehaviour<{fsmName}State>";

			if (!string.IsNullOrWhiteSpace(namespaceName))
			{
				sb.AppendLine($"namespace {namespaceName}");
				sb.AppendLine("{");

				for (int i = 0; i < states.Count; i++)
				{
					string state = states[i];

					sb.AppendLine($"\tpublic class {fsmName}{state}StateBehaviour : {baseClass}");
					sb.AppendLine("\t{");
					sb.AppendLine($"\t\tpublic override {fsmName}State State => {fsmName}State.{state};");
					sb.AppendLine();
					sb.AppendLine($"\t\tpublic override void OnEnter({fsmName}State prevState)");
					sb.AppendLine("\t\t{");
					sb.AppendLine("\t\t}");
					sb.AppendLine();
					sb.AppendLine($"\t\tpublic override void OnExit({fsmName}State nextState)");
					sb.AppendLine("\t\t{");
					sb.AppendLine("\t\t}");
					sb.AppendLine();
					sb.AppendLine("\t\tpublic override void OnUpdateState()");
					sb.AppendLine("\t\t{");
					sb.AppendLine("\t\t}");
					sb.AppendLine("\t}");

					if (i < states.Count - 1)
					{
						sb.AppendLine();
					}
				}

				sb.AppendLine("}");
			}
			else
			{
				for (int i = 0; i < states.Count; i++)
				{
					string state = states[i];

					sb.AppendLine($"public class {fsmName}{state}StateBehaviour : {baseClass}");
					sb.AppendLine("{");
					sb.AppendLine($"\tpublic override {fsmName}State State => {fsmName}State.{state};");
					sb.AppendLine();
					sb.AppendLine($"\tpublic override void OnEnter({fsmName}State prevState)");
					sb.AppendLine("\t{");
					sb.AppendLine("\t}");
					sb.AppendLine();
					sb.AppendLine($"\tpublic override void OnExit({fsmName}State nextState)");
					sb.AppendLine("\t{");
					sb.AppendLine("\t}");
					sb.AppendLine();
					sb.AppendLine("\tpublic override void OnUpdateState()");
					sb.AppendLine("\t{");
					sb.AppendLine("\t}");
					sb.AppendLine("}");

					if (i < states.Count - 1)
					{
						sb.AppendLine();
					}
				}
			}

			return sb.ToString();
		}
	}
}
