using System;
using _ProjectX.Scripts.Data.ScriptableObjects;
using UnityEngine;

namespace Player
{
	public class CarSetup : MonoBehaviour
	{
		public SO_CarSettings settings;

		private DriveController driveController;
		private Rigidbody rb;

		private void Awake()
		{
			driveController = GetComponent<DriveController>();
			rb = GetComponent<Rigidbody>();

			rb.mass = settings.mass;
			rb.drag = settings.drag;
			rb.angularDrag = settings.angularDrag;
		}

		public void UpdateCar()
		{
			rb = GetComponent<Rigidbody>();
			
			rb.mass = settings.mass;
			rb.drag = settings.drag;
			rb.angularDrag = settings.angularDrag;
			
			driveController.SetupCarSettings();
		}
	}
}