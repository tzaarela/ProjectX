using Cinemachine;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal
	{
		[Header("Setup")]
		[SerializeField] private GameObject bulletPrefab;

		[Header("Settings")]
		public List<AxleInfo> axleInfos;
		public float maxMotorTorque;
		public float maxSteeringAngle;
		public Vector3 centerOfMassOffset;

		private InputManager inputs;
		private CinemachineVirtualCamera virtualCamera;
		private Rigidbody rb;
		private PowerupSlot powerupSlot;
		private float travelL = 0;
		private float travelR = 0;
		private float antiRoll = 8000;

		private void Start()
		{
			if (!isLocalPlayer)
				return;
			
			inputs = GetComponent<InputManager>();

			// virtualCamera = GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
			// virtualCamera.Follow = transform;
			
			SendGlobal(GlobalEvent.SET_FOLLOW_TARGET, new GameObjectData(gameObject));
			
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			rb = GetComponent<Rigidbody>();
			rb.centerOfMass = centerOfMassOffset;
			powerupSlot = GetComponent<PowerupSlot>();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;
			
			if (inputs.isUsingPowerup)
				UsePowerup();
		}
		
		private void FixedUpdate()
		{
			if (!isLocalPlayer)
				return;
		
			Drive();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMassOffset, 0.05f);
		}

		[Client]
		private void UsePowerup()
		{
			Vector3 shootingDirection = transform.forward;
			CmdUsePowerup(shootingDirection);
		}

		[Command]
		private void CmdUsePowerup(Vector3 shootingDirection)
		{
			//if (powerupSlot.hasPowerup)
			//{
			//	//powerupSlot.Use(); ?
			//	//powerupSlot.Power.Use(); ?
			//}

			//Move to MachineGun Powerup class
			GameObject bullet = ServiceLocator.ObjectPools.SpawnFromPool(ObjectPoolType.Bullet);
			Vector3 bulletPosition = transform.position + new Vector3(0, 2, 0);
			bullet.transform.position = bulletPosition;
			bullet.GetComponent<Bullet>().Shoot(shootingDirection);
		}

		[Client]
		private void Drive()
		{
			Vector3 direction = new Vector3(inputs.movement.x, 0, inputs.movement.y);
			CmdDrive(direction);
		}

		[Command]
		private void CmdDrive(Vector3 direction)
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

				AutoStabilize(axleInfo);

				//ApplyLocalPositionToVisuals(axleInfo.leftWheel);
				//ApplyLocalPositionToVisuals(axleInfo.rightWheel);
			}
		}

		private void AutoStabilize(AxleInfo axleInfo)
		{
			WheelHit hit;
			
			bool groundedL = axleInfo.leftWheel.GetGroundHit(out hit);
			if (groundedL)
			{
				travelL = (-axleInfo.leftWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.leftWheel.radius) / axleInfo.leftWheel.suspensionDistance;
			}

			bool groundedR = axleInfo.rightWheel.GetGroundHit(out hit);
			if (groundedR)
			{
				travelR = (-axleInfo.rightWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.rightWheel.radius) / axleInfo.rightWheel.suspensionDistance;
			}

			float antiRollForce = (travelL - travelR) * antiRoll;
			
			if (groundedL)
			{
				rb.AddForceAtPosition(axleInfo.leftWheel.transform.up * -antiRollForce,
						axleInfo.leftWheel.transform.position);
			}

			if (groundedR)
			{
				rb.AddForceAtPosition(axleInfo.rightWheel.transform.up * antiRollForce,
						axleInfo.rightWheel.transform.position);
			}
		}
		
		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}