using Audio.ScriptableObjects;
using UnityEngine;
using UnityEditor;

namespace Audio.Editor
{
	[CustomEditor(typeof(AudioEvent), true)]
	public class AudioEventEditor : UnityEditor.Editor
	{

		[SerializeField] private AudioSource previewer;

		public void OnEnable()
		{
			previewer = EditorUtility
			            .CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource))
			            .GetComponent<AudioSource>();
		}

		public void OnDisable()
		{
			DestroyImmediate(previewer.gameObject);
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
			if (GUILayout.Button("Preview"))
			{
				((AudioEvent) target).Play(previewer);
			}

			EditorGUI.EndDisabledGroup();
		}
	}
}
