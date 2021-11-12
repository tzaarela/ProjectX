using UnityEditor;
using UnityEngine;
using Player;

[CanEditMultipleObjects]
[CustomEditor(typeof(CarSetup), true)]
public class CarSettingsEditor : Editor
{
	private CarSetup carSetup;
	private bool hasComponent;

	public void OnEnable()
	{
		if (Selection.activeGameObject.HasComponent<CarSetup>())
		{
			carSetup = Selection.activeGameObject.GetComponent<CarSetup>();
			hasComponent = true;
		}
		else
		{
			hasComponent = false;
		}
	}

	public override void OnInspectorGUI()
	{
		if (hasComponent)
		{
			EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);

			EditorGUILayout.Space(20);
    
			GUILayout.BeginHorizontal();
			
			GUI.backgroundColor = Color.green;
    
			if (GUILayout.Button("Update Car"))
			{
				carSetup.UpdateCar();
			}
    
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space(20);
    
			EditorGUI.EndDisabledGroup();
		}
		
		GUI.backgroundColor = Color.white;
		
		DrawDefaultInspector();

		if (!hasComponent)
			return;
    
		EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);

		EditorGUILayout.Space(20);
    
		GUILayout.BeginHorizontal();
    
		GUI.backgroundColor = Color.green;
		
		if (GUILayout.Button("Update Car"))
		{
			carSetup.UpdateCar();
		}
    
		GUILayout.EndHorizontal();
    
		EditorGUI.EndDisabledGroup();
	}
}
