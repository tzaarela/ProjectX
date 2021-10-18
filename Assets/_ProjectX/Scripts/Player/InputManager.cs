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
		public bool isUsingPowerup;
		public bool isDroppingPowerup;

		private void Start()
		{
			playerControls = new PlayerControls();
			playerControls.Player.Drive.performed += DrivePerformed;
			playerControls.Player.Drive.canceled += DrivePerformed;
			playerControls.Player.Powerup.started += UsePowerStarted;
			playerControls.Player.Powerup.canceled += UsePowerCanceled;
			playerControls.Player.Drop.started += DropPowerStarted;
			playerControls.Player.Drop.canceled += DropPowerCanceled;
			playerControls.Enable();
		}

		private void UsePowerStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			isUsingPowerup = true;
		}

		private void UsePowerCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			isUsingPowerup = false;
		}

		private void DropPowerStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			isDroppingPowerup = true;
		}
		
		private void DropPowerCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			isDroppingPowerup = false;
		}

		private void DrivePerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			 movement = obj.ReadValue<Vector2>();
		}
	}
}