using Mirror;
using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using PowerUp.Projectiles;
using UnityEngine;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal
	{
		[Header("Settings")]
		public List<AxleInfo> axleInfos;
		public float maxMotorTorque;
		public float maxSteeringAngle;
		public Vector3 centerOfMassOffset;

		[Header("References")]
		[SerializeField] private InputManager inputs;
		[SerializeField] private PowerupController powerupSlot;
		[SerializeField] private Rigidbody rb;
		
		private float travelL = 0;
		private float travelR = 0;
		private float antiRoll = 8000;

		public override void OnStartClient()
		{
			base.OnStartClient();

			print("OnStartClient(netId) " + GetComponent<NetworkIdentity>().netId);
			rb.centerOfMass = centerOfMassOffset;

			if (!isLocalPlayer)
				return;
			
			SendGlobal(GlobalEvent.SET_FOLLOW_TARGET, new GameObjectData(gameObject));
			CmdUpdateActivePlayersList();

			name += "-local";
		}

		[Command]
		private void CmdUpdateActivePlayersList()
		{
			ServiceLocator.RoundManager.AddActivePlayer();
		}

		//private void Update()
		//{
		//	if (!isLocalPlayer)
		//		return;
		//}

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