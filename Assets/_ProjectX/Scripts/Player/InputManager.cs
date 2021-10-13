using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class InputManager : MonoBehaviour
	{
		public PlayerControls playerControls;

		public Vector2 movement = Vector2.zero;
		public bool isBoosting;
		public bool isBraking;
		public bool isShooting;

		private void Start()
		{
			playerControls = new PlayerControls();
			playerControls.Player.Move.performed += MovePerformed;
			playerControls.Player.Move.canceled += MovePerformed;
			playerControls.Player.Fire.started += FireStarted;
			playerControls.Player.Fire.canceled += FireCanceled;
			playerControls.Enable();
		}

		private void FireStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			isShooting = true;
		}

		private void FireCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			isShooting = false;
		}

		private void MovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			 movement = obj.ReadValue<Vector2>();
		}
	}
}