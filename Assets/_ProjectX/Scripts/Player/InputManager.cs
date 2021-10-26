using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class InputManager : MonoBehaviour
	{
		public PlayerControls playerControls;

		public float acceleration = 0;
		public float steering = 0;
		public bool isBoosting;
		public bool isBraking;
		public bool isUsingPowerup;
		public bool isDroppingPowerup;

		public void Awake()
		{
			// PlayerControls is enabled later when all players are connected to the game
			playerControls = new PlayerControls();
			playerControls.Player.Accelerate.performed += Accelerate;
			playerControls.Player.Accelerate.canceled += Accelerate;
			playerControls.Player.Steer.performed += Steer;
			playerControls.Player.Steer.canceled += Steer;
			playerControls.Player.Handbrake.performed += Handbrake;
			playerControls.Player.Handbrake.canceled += Handbrake;
			playerControls.Player.Boost.performed += Boost;
			playerControls.Player.Boost.canceled += Boost;
			playerControls.Player.Powerup.started += UsePowerStarted;
			playerControls.Player.Powerup.canceled += UsePowerCanceled;
			playerControls.Player.Drop.started += DropPowerStarted;
			playerControls.Player.Drop.canceled += DropPowerCanceled;
		}

		private void Boost(InputAction.CallbackContext obj)
		{
			isBoosting = obj.performed;
		}

		private void Steer(InputAction.CallbackContext obj)
		{
			steering = obj.ReadValue<float>();
		}

		private void Accelerate(InputAction.CallbackContext obj)
		{
			 acceleration = obj.ReadValue<float>();
		}

		private void Handbrake(InputAction.CallbackContext obj)
		{
			isBraking = obj.performed;
		}

		private void UsePowerStarted(InputAction.CallbackContext obj)
		{
			isUsingPowerup = true;
		}

		private void UsePowerCanceled(InputAction.CallbackContext obj)
		{
			isUsingPowerup = false;
		}

		private void DropPowerStarted(InputAction.CallbackContext obj)
		{
			isDroppingPowerup = true;
		}
		
		private void DropPowerCanceled(InputAction.CallbackContext obj)
		{
			isDroppingPowerup = false;
		}

		public void EnableInput()
		{
			playerControls.Enable();
		}
		
		public void DisableInput()
		{
			playerControls.Disable();
		}
	}
}