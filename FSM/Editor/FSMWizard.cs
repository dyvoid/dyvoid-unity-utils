using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace dyvoid.Utility.Editor
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

		private string ApplyTemplate(string template)
		{
			template = template.Replace("#FSM_NAME#", fsmName);

			if (string.IsNullOrWhiteSpace(namespaceName))
			{
				template = template
					.Replace("#NAMESPACE_START#\r\n", "")
					.Replace("#NAMESPACE_START#\n", "")
					.Replace("#NAMESPACE_END#", "");

				// Remove one level of indentation
				template = template.Replace("\r\n\t", "\r\n").Replace("\n\t", "\n");

				if (template.StartsWith("\t"))
				{
					template = template.Substring(1);
				}
				
				return template;
			}
			else
			{
				return template
					.Replace("#NAMESPACE_START#", $"namespace {namespaceName}\n{{")
					.Replace("#NAMESPACE_END#", "}");
			}
		}

		private string GenerateEnum()
		{
			string template = FSMWizardTemplates.EnumTemplate;

			var statesList = new System.Collections.Generic.List<string>();
			foreach (string state in states)
			{
				statesList.Add($"\t\t{state},");
			}
			
			template = template.Replace("#STATES#", string.Join("\n", statesList));
			return ApplyTemplate(template);
		}

		private string GenerateFSMClass()
		{
			string template = FSMWizardTemplates.FSMClassTemplate;

			return ApplyTemplate(template);
		}

		private string GenerateBlackboard()
		{
			string template = FSMWizardTemplates.BlackboardTemplate;

			return ApplyTemplate(template);
		}

		private string GenerateVisibility()
		{
			string template = FSMWizardTemplates.VisibilityTemplate;

			return ApplyTemplate(template);
		}

		private string GenerateStateBehaviour(string state)
		{
			string template = FSMWizardTemplates.StateBehaviourTemplate;

			string baseClass = generateBlackboard
				? $"FSMStateBehaviour<#FSM_NAME#State, #FSM_NAME#Blackboard>"
				: $"FSMStateBehaviour<#FSM_NAME#State>";

			template = template
				.Replace("#STATE#", state)
				.Replace("#BASE_CLASS#", baseClass);

			return ApplyTemplate(template);
		}

		private string GenerateAllStateBehaviours()
		{
			string template = FSMWizardTemplates.AllStateBehavioursTemplate;
			string classTemplate = FSMWizardTemplates.SingleStateClassTemplate;

			string baseClass = generateBlackboard
				? $"FSMStateBehaviour<#FSM_NAME#State, #FSM_NAME#Blackboard>"
				: $"FSMStateBehaviour<#FSM_NAME#State>";

			var classesList = new System.Collections.Generic.List<string>();
			foreach (string state in states)
			{
				classesList.Add(classTemplate
					.Replace("#STATE#", state)
					.Replace("#BASE_CLASS#", baseClass));
			}

			template = template.Replace("#STATE_CLASSES#", string.Join("\n\n", classesList));

			return ApplyTemplate(template);
		}
	}
}
