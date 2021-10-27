using System;
using _ProjectX.Scripts.Data.ScriptableObjects;
using UnityEngine;

namespace Player
{
	public class CarSetup : MonoBehaviour
	{
		public SO_CarSettings settings;

		private Rigidbody rb;

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();

			rb.mass = settings.mass;
			rb.drag = settings.drag;
			rb.angularDrag = settings.angularDrag;
		}
	}
}