using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class PlayerController : NetworkBehaviour
	{
		[Header("Settings")]
		public float speed;

		private InputManager inputs;

		private void Start()
		{
			if (!isLocalPlayer)
				return;

			inputs = GetComponent<InputManager>();
		}

		private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			throw new System.NotImplementedException();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;

				Move();
		}

		[Client]
		private void Move()
		{
			Vector3 direction = new Vector3(inputs.movement.x, 0, inputs.movement.y);
			CmdMove(direction);
		}

		[Command]
		private void CmdMove(Vector3 direction)
		{
			transform.position += direction * speed * Time.deltaTime;
		}
	}
}