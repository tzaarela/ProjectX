using Data.Enums;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Player
{
	public class DriveController : NetworkBehaviour
	{
		[Header("Settings")]
		public List<CarAxle> axleInfos;
		public List<FrictionCurve> frictionCurves;
		public float maxMotorTorque;
		public float maxSteeringAngle;
		public float brakeTorque;
		public float decelerationForce;
		public float boostMultiplier = 6f;
		public float antiRoll = 8000;
		public Vector3 centerOfMassOffset;

		[Header("References")]
		[SerializeField] private ParticleSystem boostParticle;

		private InputManager inputs;
		private float travelL = 0;
		private float travelR = 0;
		private float defaultMaxMotorTorque;
		private WheelFrictionCurve sidewayFrictionCurveNormal;
		private WheelFrictionCurve sidewayFrictionCurveHandbrake;
		private WheelFrictionCurve forwardFrictionCurveHandbrake;
		private WheelFrictionCurve forwardFrictionCurveNormal;

		private Rigidbody rb;

		private void Start()
		{
			if (!isServer)
				return;
			
			defaultMaxMotorTorque = maxMotorTorque;
			rb = GetComponent<Rigidbody>();
			rb.centerOfMass = centerOfMassOffset;

			CreateFrictionCurves();
			SetNormalFriction();
		}

		[Server]
		private void CreateFrictionCurves()
		{
			CreateNormalFriction();
			CreateHandbrakeFriction();
		}

		[Server]
		private void CreateHandbrakeFriction()
		{
			FrictionCurve frictionValueHandbrake = frictionCurves.FirstOrDefault(x => x.key == FrictionType.handbrake);
			sidewayFrictionCurveHandbrake = CreateSidewayFrictionCurve(frictionValueHandbrake);
			forwardFrictionCurveHandbrake = CreateForwardFrictionCurve(frictionValueHandbrake);
		}

		[Server]
		private void CreateNormalFriction()
		{
			FrictionCurve frictionValueNormal = frictionCurves.FirstOrDefault(x => x.key == FrictionType.normal);
			sidewayFrictionCurveNormal = CreateSidewayFrictionCurve(frictionValueNormal);
			forwardFrictionCurveNormal = CreateForwardFrictionCurve(frictionValueNormal);
		}

		[Server]
		private void SetNormalFriction()
		{
			foreach (CarAxle axle in axleInfos)
			{
				axle.leftWheel.forwardFriction = forwardFrictionCurveNormal;
				axle.rightWheel.forwardFriction = forwardFrictionCurveNormal;
				axle.leftWheel.sidewaysFriction = sidewayFrictionCurveNormal;
				axle.rightWheel.sidewaysFriction = sidewayFrictionCurveNormal;
			}
		}

		public override void OnStartClient()
		{
			if (!isLocalPlayer)
				return;

			if (inputs == null)
				inputs = GetComponent<InputManager>();

			inputs.playerControls.Player.Boost.performed += Boost_performed;
			inputs.playerControls.Player.Boost.canceled += Boost_canceled;
			inputs.playerControls.Player.Handbrake.performed += Brake; 
			inputs.playerControls.Player.Handbrake.canceled += Brake;
		}

		private void FixedUpdate()
		{
			if (!isLocalPlayer)
				return;

			if (inputs == null)
				inputs = GetComponent<InputManager>();

			Drive();
		}
		
		[Client]
		private void Brake(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			CmdBrake(obj.performed);
		}

		[Command]
		private void CmdBrake(bool isHoldingHandbrake)
		{
			foreach (CarAxle axel in axleInfos)
			{
				if (axel.hasHandbrake)
				{
					if (isHoldingHandbrake)
					{
						axel.leftWheel.sidewaysFriction = sidewayFrictionCurveHandbrake;
						axel.rightWheel.sidewaysFriction = sidewayFrictionCurveHandbrake;
						axel.leftWheel.forwardFriction = forwardFrictionCurveHandbrake;
						axel.rightWheel.forwardFriction = forwardFrictionCurveHandbrake;
						axel.leftWheel.brakeTorque = brakeTorque;
						axel.rightWheel.brakeTorque = brakeTorque;
					}
					else
					{
						axel.leftWheel.sidewaysFriction = sidewayFrictionCurveNormal;
						axel.rightWheel.sidewaysFriction = sidewayFrictionCurveNormal;
						axel.leftWheel.forwardFriction = forwardFrictionCurveNormal;
						axel.rightWheel.forwardFriction = forwardFrictionCurveNormal;
						axel.leftWheel.brakeTorque = 0;
						axel.rightWheel.brakeTorque = 0;
					}
				}
			}
		}

		[Server]
		private WheelFrictionCurve CreateSidewayFrictionCurve(FrictionCurve friction)
		{
			WheelFrictionCurve frictionCurve = new WheelFrictionCurve();
			frictionCurve.asymptoteSlip = friction.sidewayFriction.asymptoteSlip;
			frictionCurve.asymptoteValue = friction.sidewayFriction.asymptoteValue;
			frictionCurve.extremumSlip = friction.sidewayFriction.extremumSlip;
			frictionCurve.extremumValue = friction.sidewayFriction.extremumValue;
			frictionCurve.stiffness = friction.sidewayFriction.stiffness;
			return frictionCurve;
		}

		[Server]
		private WheelFrictionCurve CreateForwardFrictionCurve(FrictionCurve friction)
		{
			WheelFrictionCurve frictionCurve = new WheelFrictionCurve();
			frictionCurve.asymptoteSlip = friction.forwardFriction.asymptoteSlip;
			frictionCurve.asymptoteValue = friction.forwardFriction.asymptoteValue;
			frictionCurve.extremumSlip = friction.forwardFriction.extremumSlip;
			frictionCurve.extremumValue = friction.forwardFriction.extremumValue;
			frictionCurve.stiffness = friction.forwardFriction.stiffness;
			return frictionCurve;
		}

		[Client]
		private void Drive()
		{
			if (NetworkClient.ready)
				CmdDrive(inputs.acceleration, inputs.steering);
		}

		[Command]
		private void CmdDrive(float acceleration, float steer)
		{
			float motor = maxMotorTorque * acceleration;
			float steering = maxSteeringAngle * steer;

			foreach (CarAxle axleInfo in axleInfos)
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

		[Server]
		private void AutoStabilize(CarAxle axleInfo)
		{
			WheelHit hit;

			bool groundedL = axleInfo.leftWheel.GetGroundHit(out hit);
			if (groundedL)
				travelL = (-axleInfo.leftWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.leftWheel.radius) / axleInfo.leftWheel.suspensionDistance;

			bool groundedR = axleInfo.rightWheel.GetGroundHit(out hit);
			if (groundedR)
				travelR = (-axleInfo.rightWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.rightWheel.radius) / axleInfo.rightWheel.suspensionDistance;

			float antiRollForce = (travelL - travelR) * antiRoll;

			if (groundedL)
				rb.AddForceAtPosition(axleInfo.leftWheel.transform.up * -antiRollForce,
						axleInfo.leftWheel.transform.position);

			if (groundedR)
				rb.AddForceAtPosition(axleInfo.rightWheel.transform.up * antiRollForce,
						axleInfo.rightWheel.transform.position);
		}

		[Client]
		private void Boost_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			if (inputs.acceleration < 0)
			{
				Boost(false);
				return;
			}

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

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMassOffset, 0.05f);
		}
	}
}
