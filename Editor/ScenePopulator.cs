using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScenePopulator : EditorWindow
{
	private const string RootName = "SceneProps";

	private int seed = 42;
	private int ringCount = 3;
	private int objectsPerRing = 12;
	private int floatingObjectCount = 20;
	private float ringSpacingBase = 3f;

	[MenuItem("Tools/Scene Populator")]
	private static void OpenWindow()
	{
		GetWindow<ScenePopulator>("Scene Populator");
	}

	private void OnGUI()
	{
		GUILayout.Label("Primitive Scene", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		seed = EditorGUILayout.IntField("Seed", seed);
		ringCount = EditorGUILayout.IntSlider("Ring Count", ringCount, 1, 6);
		objectsPerRing = EditorGUILayout.IntSlider("Objects Per Ring", objectsPerRing, 4, 24);
		floatingObjectCount = EditorGUILayout.IntSlider("Floating Objects", floatingObjectCount, 0, 60);
		ringSpacingBase = EditorGUILayout.Slider("Ring Spacing", ringSpacingBase, 1.5f, 8f);

		EditorGUILayout.Space();

		if (GUILayout.Button("Populate Scene", GUILayout.Height(36)))
		{
			PopulateScene();
		}

		if (GUILayout.Button("Clear Scene"))
		{
			ClearScene();
		}
	}

	private void PopulateScene()
	{
		ClearScene();

		var root = new GameObject(RootName);
		Undo.RegisterCreatedObjectUndo(root, "Populate Scene");

		var rng = new System.Random(seed);

		SpawnRings(root, rng);
		SpawnFloatingObjects(root, rng);
		SpawnGroundMarkers(root, rng);
	}

	private void SpawnRings(GameObject root, System.Random rng)
	{
		var primitiveTypes = new[]
		{
			PrimitiveType.Cube,
			PrimitiveType.Cylinder,
			PrimitiveType.Capsule,
			PrimitiveType.Sphere,
		};

		for (var ring = 0; ring < ringCount; ring++)
		{
			var radius = ringSpacingBase * (ring + 1);
			var ringParent = new GameObject($"Ring_{ring + 1}");
			ringParent.transform.SetParent(root.transform);

			for (var i = 0; i < objectsPerRing; i++)
			{
				var angle = (360f / objectsPerRing) * i + (float)rng.NextDouble() * 15f;
				var rad = angle * Mathf.Deg2Rad;
				var x = Mathf.Sin(rad) * radius;
				var z = Mathf.Cos(rad) * radius;

				var type = primitiveTypes[rng.Next(primitiveTypes.Length)];
				var obj = GameObject.CreatePrimitive(type);

				RemoveCollider(obj);

				var scaleX = 0.3f + (float)rng.NextDouble() * 0.7f;
				var scaleY = 0.5f + (float)rng.NextDouble() * 3.5f;
				var scaleZ = 0.3f + (float)rng.NextDouble() * 0.7f;

				obj.transform.SetParent(ringParent.transform);
				obj.transform.localPosition = new Vector3(x, scaleY * 0.5f, z);
				obj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
				obj.transform.localRotation = Quaternion.Euler(
					0f,
					(float)rng.NextDouble() * 360f,
					0f
				);

				obj.name = $"{type}_{ring}_{i}";
			}
		}
	}

	private void SpawnFloatingObjects(GameObject root, System.Random rng)
	{
		var primitiveTypes = new[]
		{
			PrimitiveType.Cube,
			PrimitiveType.Sphere,
			PrimitiveType.Capsule,
		};

		var floatParent = new GameObject("Floating");
		floatParent.transform.SetParent(root.transform);

		var maxRadius = ringSpacingBase * ringCount;

		for (var i = 0; i < floatingObjectCount; i++)
		{
			var type = primitiveTypes[rng.Next(primitiveTypes.Length)];
			var obj = GameObject.CreatePrimitive(type);

			RemoveCollider(obj);

			var angle = (float)rng.NextDouble() * 360f * Mathf.Deg2Rad;
			var dist = 1.5f + (float)rng.NextDouble() * maxRadius;
			var x = Mathf.Sin(angle) * dist;
			var z = Mathf.Cos(angle) * dist;
			var y = 1.5f + (float)rng.NextDouble() * 5f;

			var uniformScale = 0.15f + (float)rng.NextDouble() * 0.6f;

			obj.transform.SetParent(floatParent.transform);
			obj.transform.localPosition = new Vector3(x, y, z);
			obj.transform.localScale = Vector3.one * uniformScale;
			obj.transform.localRotation = Quaternion.Euler(
				(float)rng.NextDouble() * 360f,
				(float)rng.NextDouble() * 360f,
				(float)rng.NextDouble() * 360f
			);

			obj.name = $"Float_{type}_{i}";
		}
	}

	private void SpawnGroundMarkers(GameObject root, System.Random rng)
	{
		var groundParent = new GameObject("GroundMarkers");
		groundParent.transform.SetParent(root.transform);

		var maxRadius = ringSpacingBase * ringCount;
		var count = 30;

		for (var i = 0; i < count; i++)
		{
			var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

			RemoveCollider(obj);

			var angle = (float)rng.NextDouble() * 360f * Mathf.Deg2Rad;
			var dist = (float)rng.NextDouble() * maxRadius;
			var x = Mathf.Sin(angle) * dist;
			var z = Mathf.Cos(angle) * dist;

			var scaleXZ = 0.1f + (float)rng.NextDouble() * 0.4f;
			var scaleY = 0.02f + (float)rng.NextDouble() * 0.08f;

			obj.transform.SetParent(groundParent.transform);
			obj.transform.localPosition = new Vector3(x, scaleY * 0.5f, z);
			obj.transform.localScale = new Vector3(scaleXZ, scaleY, scaleXZ);
			obj.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);

			obj.name = $"Ground_{i}";
		}
	}

	private static void RemoveCollider(GameObject obj)
	{
		var col = obj.GetComponent<Collider>();

		if (col != null)
		{
			DestroyImmediate(col);
		}
	}

	private static void ClearScene()
	{
		var existing = GameObject.Find(RootName);

		if (existing != null)
		{
			Undo.DestroyObjectImmediate(existing);
		}
	}

}
