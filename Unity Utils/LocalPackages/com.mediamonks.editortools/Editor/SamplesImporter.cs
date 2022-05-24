using System.IO;
using System.Linq;

namespace MediaMonks.EditorTools
{
	using UnityEditor;
	using UnityEngine;

	public class SamplesImporter : ScriptableWizard
	{
		private SampleData[] _samples;
		private const string _menuItem = "Tools/MediaMonks/Samples Importer...";
		private const string _samplesFolder = "Samples~";
		private const string _samplesProjectFolder = "Samples";

		[MenuItem(_menuItem)]
		private static void CreateWizard()
		{
			DisplayWizard<SamplesImporter>("Import Samples", "Import");
		}

		private void OnEnable()
		{
			_samples = GetSamples();
		}

		private void OnWizardCreate()
		{
			var selectedSamples = _samples.Where(s => s.Selected).ToArray();
			if (selectedSamples.Length == 0) return;

			foreach (var sample in selectedSamples)
			{
				var targetFolder = Path.Combine(Application.dataPath, _samplesProjectFolder, sample.Name);
				FileUtil.ReplaceDirectory(sample.SourcePath, targetFolder);
			}

			AssetDatabase.Refresh();
		}

		protected override bool DrawWizardGUI()
		{
			if (_samples.Length == 0)
			{
				EditorGUILayout.HelpBox("No samples found in the project.\n\n" +
				                        "Add subfolders in a 'Samples~' folder in the root of your project." +
				                        "Each subfolder will appear as a sample here.", 
				                        MessageType.Info);
				return false;
			}
			
			EditorGUILayout.LabelField("Select Samples", EditorStyles.boldLabel);
			EditorGUILayout.Separator();

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				foreach (var sample in _samples)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						sample.Selected = EditorGUILayout.ToggleLeft($"{sample.Name}", sample.Selected);
						EditorGUILayout.LabelField($"{sample.Size}kB", EditorStyles.miniLabel);
					}
				}

				return check.changed;
			}
		}

		private static SampleData[] GetSamples()
		{
			var samplesFolder = Path.Combine(Application.dataPath, _samplesFolder);
			if (!Directory.Exists(samplesFolder)) return new SampleData[0];

			return Directory.GetDirectories(samplesFolder)
			                .Select(d => new SampleData(d))
			                .ToArray();
		}

		private void OnWizardUpdate()
		{
			helpString = _samples.Length == 0
				? "No samples found."
				: "Please select the samples you want to import into the project.";
		}

		private class SampleData
		{
			public string SourcePath { get; }
			public string Name { get; }
			public long Size { get; }
			public bool Selected { get; set; }

			public SampleData(string sourcePath)
			{
				SourcePath = sourcePath;
				var info = new DirectoryInfo(sourcePath);

				Name = info.Name;
				Size = info.EnumerateFiles("*", SearchOption.AllDirectories)
				           .Sum(f => f.Length) / 1024;
			}
		}
	}
}