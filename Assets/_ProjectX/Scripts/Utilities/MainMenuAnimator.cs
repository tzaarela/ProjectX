using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimator : MonoBehaviour
{
    public List<GameObject> activateObjectsInList;

	private void Start()
	{
		ActivateObjects();
	}

	private void ActivateObjects()
	{
		foreach (GameObject obj in activateObjectsInList)
		{
			obj.SetActive(true);
		}
	}
}
