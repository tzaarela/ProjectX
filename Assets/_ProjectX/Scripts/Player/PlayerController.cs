using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class PlayerController : NetworkBehaviour
	{
		[Header("Setup")]
		[SerializeField] private GameObject bulletPrefab;

		[Header("Settings")]
		public float speed;

		private InputManager inputs;
		private Cinemachine.CinemachineVirtualCamera virtualCamera;

		private void Start()
		{
			if (!isLocalPlayer)
				return;

			inputs = GetComponent<InputManager>();
			virtualCamera = GameObject.Find("VirtualCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>();
			virtualCamera.Follow = transform;
		}

		private void FixedUpdate()
		{
			if (!isLocalPlayer)
				return;

			Move();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;

			if(inputs.isShooting)
				Shoot();
		}

		[Client]
		private void Shoot()
		{
			Vector3 shootingDirection = transform.forward;
			CmdShoot(shootingDirection);
		}

		[Command]
		private void CmdShoot(Vector3 shootingDirection)
		{
			GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
			NetworkServer.Spawn(bullet);
			bullet.GetComponent<Bullet>().Shoot(shootingDirection);
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
			transform.position += direction * speed * Time.fixedDeltaTime;
		}
	}
}