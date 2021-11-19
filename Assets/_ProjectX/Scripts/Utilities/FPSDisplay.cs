using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilites
{
	public class FPSDisplay : MonoBehaviour
	{
		float deltaTime = 0.0f;
		public bool showFPS = false;

		private float[] fpsArray = new float[50];
		private int index = 0;
		private float averageFPS;
		private float minFPS;
		private float maxFPS;
		private float timeUntilFPSCalculations = 5;
		private float counter;

		private string gpuName;

		Color textColor = Color.magenta;

		private InputManager inputManager;

		private void Start()
		{
			gpuName = SystemInfo.graphicsDeviceName;
			inputManager = GetComponent<InputManager>();
		}

		void Update()
		{
#if ENABLE_INPUT_SYSTEM
			if (Keyboard.current.f12Key.wasPressedThisFrame)
			{
				showFPS = !showFPS;
			}
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F12))
            {   
                showFPS = !showFPS;
            }
#endif

			deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		}

		void OnGUI()
		{
			int w = Screen.width, h = Screen.height;

			GUIStyle style = new GUIStyle();

			Rect rect = new Rect(0, 0, w, h * 2 / 100);
			Rect rect2 = new Rect(0, 30, w, h * 2 / 100);
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = h * 2 / 100;
			style.normal.textColor = textColor;
			float msec = deltaTime * 1000.0f;
			float fps = 1.0f / deltaTime;

			fpsArray[index] = fps;

			string text = "";

			if (showFPS)
			{
				text = string.Format("{0:0.0} ms ({1:0.} fps) ({2:0} average fps) min: {3:0} max: {4:0}", msec, fps, averageFPS, minFPS, maxFPS);
			}

			if (counter >= timeUntilFPSCalculations)
			{
				if (fps < minFPS)
					minFPS = fps;

				if (fps > maxFPS)
					maxFPS = fps;
			}
			else
			{
				minFPS = fps;
				maxFPS = fps;
				counter += Time.deltaTime;
			}

			if (index >= 49)
			{
				float totalFPSValues = 0;

				foreach (var fpsValue in fpsArray)
				{
					totalFPSValues += fpsValue;
					averageFPS = totalFPSValues / 50;
				}

				index = 0;
			}
			else
			{
				index++;
			}

			GUI.Label(rect, text, style);

			if (showFPS)
				GUI.Label(rect2, gpuName, style);
		}
	}
}