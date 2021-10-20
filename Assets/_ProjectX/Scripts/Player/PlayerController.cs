using Mirror;
using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using PowerUp.Projectiles;
using UnityEngine;
using System;
using Game.Flag;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal
	{
		[Header("Settings")]
		public List<AxleInfo> axleInfos;
		public float maxMotorTorque;
		public float maxSteeringAngle;
		public float brakeTorque;
		public float decelerationForce;
		public float boostMultiplier = 6f;
		public Vector3 centerOfMassOffset;

		[Header("References")]
		[SerializeField] private ParticleSystem boostParticle;
		[SerializeField] private InputManager inputs;
		[SerializeField] private PowerupController powerupSlot;
		[SerializeField] private Rigidbody rb;
		[SerializeField] private GameObject flagOnRoof;
		
		[Header("Debug")]
		[SyncVar(hook = "FlagStateChanged")] public bool hasFlag;

		private Flag flag;
		private float travelL = 0;
		private float travelR = 0;
		private float antiRoll = 8000;
		private float defaultMaxMotorTorque;
		
		private int playerId;
		
		public int PlayerId => playerId;

		public override void OnStartClient()
		{
			rb.centerOfMass = centerOfMassOffset;

			if (!isLocalPlayer)
				return;
			
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			// print("OnStartClient(netId) " + playerId);
			SendGlobal(GlobalEvent.SET_FOLLOW_TARGET, new GameObjectData(gameObject));
			CmdUpdateActivePlayersList();

			name += "-local";

		}

		private void Start()
		{
			if (!isLocalPlayer)
				return;

			inputs.playerControls.Player.Boost.performed += Boost_performed;
			inputs.playerControls.Player.Boost.canceled += Boost_canceled;
		}

		[Server]
		public void GiveFlag(Flag flag)
		{
			this.flag = flag;
			hasFlag = true;
		}

		[ContextMenu("Drop")]
		public void DropFlag()
		{
			hasFlag = false;
			flag.Drop(transform.position, rb.velocity);
		}

		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
		}

		public override void OnStartServer()
		{
			defaultMaxMotorTorque = maxMotorTorque;
		}

		[Command]
		private void CmdUpdateActivePlayersList()
		{
			ServiceLocator.RoundManager.AddActivePlayer(playerId);
		}

		private void FixedUpdate()
		{
			if (!isLocalPlayer)
				return;
		
			Drive();

			if (inputs.isBraking)
				Brake();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMassOffset, 0.05f);
		}

		[Client]
		private void Boost_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			Boost(true);
		}

		[Client]
		private void Boost_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			Boost(false);
		}

		[Client]
		private void Boost(bool turnOn)
		{
			CmdBoost(turnOn);
		}
		
		[Command]
		private void CmdBoost(bool turnOn)
		{
			if (turnOn)
			{
				maxMotorTorque = defaultMaxMotorTorque * boostMultiplier;
			}
			else
			{
				maxMotorTorque = defaultMaxMotorTorque;
			}

			RpcToggleParticle(turnOn);
		}

		[ClientRpc]
		private void RpcToggleParticle(bool turnOn)
		{
			ParticleSystem.EmissionModule em = boostParticle.emission;
			if (turnOn)
				em.enabled = true;
			else
				em.enabled = false;
		}

		[Client]
		private void Brake()
		{
			CmdBrake();
		}

		[Command]
		private void CmdBrake()
		{
			foreach (AxleInfo axel in axleInfos)
			{
				if (axel.hasHandbrake)
				{
					axel.leftWheel.brakeTorque = brakeTorque;
					axel.rightWheel.brakeTorque = brakeTorque;
				}
			}
		}

		[Client]
		private void Drive()
		{
			CmdDrive(inputs.acceleration, inputs.steering);
		}

		[Command]
		private void CmdDrive(float acceleration, float steer)
		{
			float motor = maxMotorTorque * acceleration;
			float steering = maxSteeringAngle * steer;

			foreach (AxleInfo axleInfo in axleInfos)
			{
				if (axleInfo.hasSteering)
				{
					axleInfo.leftWheel.steerAngle = steering;
					axleInfo.rightWheel.steerAngle = steering;
				}

				if (axleInfo.hasMotor)
				{
					if (motor != 0)
					{
						axleInfo.leftWheel.brakeTorque = 0;
						axleInfo.rightWheel.brakeTorque = 0;
						axleInfo.leftWheel.motorTorque = motor;
						axleInfo.rightWheel.motorTorque = motor;
					}
					else
					{
						axleInfo.leftWheel.brakeTorque = decelerationForce;
						axleInfo.rightWheel.brakeTorque = decelerationForce;
					}
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