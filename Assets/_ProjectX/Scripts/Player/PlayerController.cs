using Cinemachine;
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
		public List<AxleInfo> axleInfos;
		public float maxMotorTorque;
		public float maxSteeringAngle;

		private InputManager inputs;
		private CinemachineVirtualCamera virtualCamera;
		private Rigidbody rb;
		public Vector3 centerOfMassOffset;

		private void Start()
		{
			if (!isLocalPlayer)
				return;

			inputs = GetComponent<InputManager>();
			rb = GetComponent<Rigidbody>();
			virtualCamera = GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
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


			rb.centerOfMass = centerOfMassOffset;

			if (inputs.isShooting)
				Shoot();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMassOffset, 0.05f);
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
			
			float motor = maxMotorTorque * direction.z;
			float steering = maxSteeringAngle * direction.x;

			foreach (AxleInfo axleInfo in axleInfos)
			{
				if (axleInfo.steering)
				{
					axleInfo.leftWheel.steerAngle = steering;
					axleInfo.rightWheel.steerAngle = steering;
				}
				if (axleInfo.motor)
				{
					axleInfo.leftWheel.motorTorque = motor;
					axleInfo.rightWheel.motorTorque = motor;
				}
				//ApplyLocalPositionToVisuals(axleInfo.leftWheel);
				//ApplyLocalPositionToVisuals(axleInfo.rightWheel);
			}
		}
	}
}