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

		private void Start()
		{
			playerControls = new PlayerControls();
			playerControls.Player.Move.performed += Move_performed;
			playerControls.Player.Move.canceled += Move_performed;
			playerControls.Enable();
		}

		private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			 movement = obj.ReadValue<Vector2>();
		}
	}
}