#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace MediaMonks.EditorTools
{
	public static class DescriptionsPrinter
	{
		private struct DescriptionText
		{
			public string Name;
			public string Description;
		}

		[MenuItem("Tools/Descriptions/Create markdown of descriptions in scene...")]
		public static void PrintInScene()
		{
			var fileName = $"{Application.dataPath}/../descriptions_in_{SceneManager.GetActiveScene().name.ToSnakeCase()}.md";
			File.WriteAllText(fileName, CreateSceneDescriptionMarkdown());
			EditorUtility.OpenWithDefaultApp(fileName);
		}

		/// <summary>
		/// Creates markdown of all the descriptions in the scene and the of the prefabs that it references.
		/// </summary>
		private static string CreateSceneDescriptionMarkdown()
		{
			return new StringBuilder()
			       // Page header
			       .AppendFormat("# Descriptions in {0} scene\n", SceneManager.GetActiveScene().name)
			       .AppendFormat("{0}\n\n", SceneManager.GetActiveScene().path.FormatPath())
			       // GameObjects
			       .Append("## GameObjects in scene\n\n")
			       .AppendTableHeader("GameObject", "Description")
			       .AppendTableRowsFrom(
				       DescriptionsOnlyInScene().Select(ToDescriptionText)
				                                .Select(PrettyPrintMD)
				                                .OrderBy(text => text.Name))
			       // Prefabs
			       .Append("\n## Used Prefabs in scene\n\n")
			       .AppendTableHeader("Prefab", "Description")
			       .AppendTableRowsFrom(
				       DescriptionsNotOnlyInScene().Select(ToDescriptionText)
				                                   .Select(PrettyPrintMD)
				                                   .OrderBy(text => text.Name))
			       .ToString();
		}

		/// <summary>
		/// Appends list as MD rows.
		/// </summary>
		private static StringBuilder AppendTableRowsFrom(this StringBuilder sb, IEnumerable<DescriptionText> texts)
		{
			foreach (var text in texts)
			{
				sb.AppendTableRow(text.Name, text.Description);
			}

			return sb;
		}

		/// <summary>
		/// Appends a MD table header.
		/// </summary>
		private static StringBuilder AppendTableHeader(this StringBuilder sb, params string[] entries)
		{
			Assert.IsTrue(entries.Length > 1);

			// Append something like this "Header 0 | Header 1 | Header 2\n".
			sb.AppendTableRow(entries);

			// Append something like this "--- | --- | ---\n".
			for (var i = 0; i < entries.Length - 1; i++)
			{
				sb.Append("--- | ");
			}

			return sb.Append("---\n");
		}

		/// <summary>
		/// Appends a MD table row.
		/// </summary>
		private static StringBuilder AppendTableRow(this StringBuilder sb, params string[] entries)
		{
			Assert.IsTrue(entries.Length > 1);

			// Append something like this "Entry 0 | Entry 1 | Entry 2\n".
			for (var i = 0; i < entries.Length - 1; i++)
			{
				sb.AppendFormat("{0} | ", entries[i]);
			}

			return sb.AppendFormat("{0}\n", entries.Last());
		}

		/// <summary>
		/// Gets DescriptionText of Description.
		/// </summary>
		private static DescriptionText ToDescriptionText(this Description desc)
		{
			return new DescriptionText
			{
				Name = desc.transform.GetHierarchyPath(),
				Description = desc.Text
			};
		}

		/// <summary>
		/// Cleans up the DescriptionText for MD output.
		/// </summary>
		private static DescriptionText PrettyPrintMD(this DescriptionText text)
		{
			return new DescriptionText
			{
				Name = text.Name.Desquare().FormatPath(),
				Description = text.Description.ReplaceLineBreaks("<br>")
			};
		}

		/// <summary>
		/// Formats path for MD output.
		/// </summary>
		private static string FormatPath(this string str) => str.SpacesToNbsp().AddPathBreaks();

		/// <summary>
		/// Returns hierarchy path of the Transform.
		/// </summary>
		private static string GetHierarchyPath(this Transform current)
			=> current.parent == null ? $"{current.name}" : $"{current.parent.GetHierarchyPath()}/{current.name}";

		/// <summary>
		/// Removes square brackets ('[', ']').
		/// </summary>
		private static string Desquare(this string str) => Regex.Replace(str, @"(\[|\])", "").Trim();

		/// <summary>
		/// Converts spaces to non-breaking spaces.
		/// </summary>
		private static string SpacesToNbsp(this string str) => Regex.Replace(str, @"(\s)", "&nbsp;");

		/// <summary>
		/// The ZWSP unicode character.
		/// </summary>
		private const string Zwsp = "​";

		/// <summary>
		/// Adds optional breaks in path.
		/// </summary>
		private static string AddPathBreaks(this string str) => str.Replace("/", "/" + Zwsp);

		/// <summary>
		/// Replaces line breaks.
		/// </summary>
		private static string ReplaceLineBreaks(this string str, string with) => Regex.Replace(str, @"(\n|\r|\n\r)", with);

		/// <summary>
		/// Converts to snake_case.
		/// </summary>
		private static string ToSnakeCase(this string str) => str.Replace(' ', '_').ToLower();

		private static IEnumerable<Description> DescriptionsOnlyInScene()
		{
			return (Resources.FindObjectsOfTypeAll(typeof(Description)) as Description[])?.Where(go =>
				!EditorUtility.IsPersistent(go.transform.root.gameObject) &&
				!(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave));
		}

		private static IEnumerable<Description> DescriptionsNotOnlyInScene()
		{
			return (Resources.FindObjectsOfTypeAll(typeof(Description)) as Description[])?.Where(go =>
				EditorUtility.IsPersistent(go.transform.root.gameObject) &&
				!(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave));
		}
	}
}
#endif