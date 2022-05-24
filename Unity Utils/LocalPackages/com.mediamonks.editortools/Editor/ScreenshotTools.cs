using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MediaMonks.EditorTools
{
	public static class ScreenshotTools
	{
		private const string _menuItem = "Tools/MediaMonks/";
		
		/// <summary>
		/// Take a screenshot of the game view in the editor. Screenshots will be placed in the root of the project
		/// and numbered automatically 
		/// </summary>
		[MenuItem(_menuItem + "Take Screenshot &#s")]
		public static void TakeScreenshot()
		{
			var file = $"screenshot-{DateTime.Now:yyyyMMdd}-{DateTime.Now.TimeOfDay.TotalSeconds:000000}.png";
			ScreenCapture.CaptureScreenshot(file);
			Debug.Log($"Captured {Path.GetFullPath(file)}");
		}
	}
}
