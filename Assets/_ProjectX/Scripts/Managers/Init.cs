using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
	private void Awake()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 300;
	}
}
