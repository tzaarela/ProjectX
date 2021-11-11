using System;
using System.Collections;
using Data.Enums;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using _ProjectX.Scripts.Data.ScriptableObjects;
using Managers;
using UnityEditor;
using UnityEngine;

namespace Player
{
	public class DriveController : NetworkBehaviour
	{
		[Header("Settings")]
		public List<CarAxle> axleInfos;

		[Header("References")]
		[SerializeField] private ParticleSystem driftParticleLeft;
		[SerializeField] private ParticleSystem driftParticleRight;
		[SerializeField] private ParticleSystem boostParticleLeft;
		[SerializeField] private ParticleSystem boostParticleRight;
		
		[SyncVar(hook = nameof(ToggleDriftingEffects))] private bool isDrifting;
		
		private InputManager inputs;
		private float travelL = 0;
		private float travelR = 0;
		private float defaultMaxMotorTorque;
		private float maxMotorTorque;
		private float maxVelocity;

		private WheelFrictionCurve sidewayFrictionCurveNormal;
		private WheelFrictionCurve sidewayFrictionCurveHandbrake;
		private WheelFrictionCurve forwardFrictionCurveHandbrake;
		private WheelFrictionCurve forwardFrictionCurveNormal;

		private SO_CarSettings carSettings;
		private Rigidbody rb;
		private PlayerSound playerSound;

		private float remainingBoost;
		private Coroutine boostCounterRoutine;

		private void Awake()
		{
			playerSound = GetComponent<PlayerSound>();
		}

		public override void OnStartClient()
		{
			if (!isLocalPlayer)
				return;

	
			inputs = GetComponent<InputManager>();

			inputs.playerControls.Player.Boost.performed += Boost_performed;
			inputs.playerControls.Player.Boost.canceled += Boost_canceled;
			inputs.playerControls.Player.Handbrake.performed += Brake;
			inputs.playerControls.Player.Handbrake.canceled += Brake;
		}

		private void Start()
		{
			if (!isServer)
				return;

			carSettings = GetComponent<CarSetup>().settings;
			remainingBoost = carSettings.boostMaxTime;

			SetWheelColliders();
			
			defaultMaxMotorTorque = carSettings.maxMotorTorque;
			maxMotorTorque = carSettings.maxMotorTorque;
			maxVelocity = carSettings.maxVelocity;

			rb = GetComponent<Rigidbody>();
			rb.centerOfMass = carSettings.centerOfMassOffset;
			
			CreateFrictionCurves();
			SetNormalFriction();
		}

		[Server]
		private void SetWheelColliders()
		{
			foreach (CarAxle axle in axleInfos)
			{
				axle.leftWheel.mass = carSettings.wheelMass;
				axle.rightWheel.mass = carSettings.wheelMass;
				axle.leftWheel.radius = carSettings.wheelRadius;
				axle.rightWheel.radius = carSettings.wheelRadius;
				axle.leftWheel.wheelDampingRate = carSettings.wheelDampingRate;
				axle.rightWheel.wheelDampingRate = carSettings.wheelDampingRate;
				axle.leftWheel.suspensionDistance = carSettings.wheelSuspensionDistance;
				axle.rightWheel.suspensionDistance = carSettings.wheelSuspensionDistance;
				axle.leftWheel.forceAppPointDistance = carSettings.wheelForceAppPointDistance;
				axle.rightWheel.forceAppPointDistance = carSettings.wheelForceAppPointDistance;

				JointSpring suspensionSpring = new JointSpring();
					suspensionSpring.spring = carSettings.wheelSuspensionSpring;
					suspensionSpring.damper = carSettings.wheelSuspensionDamper;
					suspensionSpring.targetPosition = carSettings.wheelSuspensionTargetPosition;

				axle.leftWheel.suspensionSpring = suspensionSpring;
				axle.rightWheel.suspensionSpring = suspensionSpring;
			}
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
			FrictionCurve frictionValueHandbrake = carSettings.frictionCurves.FirstOrDefault(x => x.key == FrictionType.handbrake);
			sidewayFrictionCurveHandbrake = CreateSidewayFrictionCurve(frictionValueHandbrake);
			forwardFrictionCurveHandbrake = CreateForwardFrictionCurve(frictionValueHandbrake);
		}

		[Server]
		private void CreateNormalFriction()
		{
			FrictionCurve frictionValueNormal = carSettings.frictionCurves.FirstOrDefault(x => x.key == FrictionType.normal);
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
		
		private void FixedUpdate()
		{
			if (!isLocalPlayer)
				return;
			
			// if (inputs == null)
			// 	inputs = GetComponent<InputManager>();

			Drive();
		}

		[Client]
		private void Drive()
		{
			// NECCESSARY??
			// if (NetworkClient.ready)
			CmdDrive(inputs.acceleration, inputs.steering);
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
						axel.leftWheel.brakeTorque = carSettings.handBrakeTorque;
						axel.rightWheel.brakeTorque = carSettings.handBrakeTorque;
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

		//Hook
		private void ToggleDriftingEffects(bool oldValue, bool newValue)
		{
			if (newValue)
			{
				driftParticleRight.Play();
				driftParticleLeft.Play();
				playerSound.PlayDriftSound();
			}
			else
			{
				driftParticleLeft.Stop();
				driftParticleRight.Stop();
				playerSound.StopDriftSound();
			}
		}

		[Command]
		private void CmdDrive(float acceleration, float steer)
		{
			ApplyTorque(acceleration, steer);
			
			if (AreWeDrifting())
			{
				isDrifting = true;
			}
			else if (driftParticleLeft.isPlaying || driftParticleRight.isPlaying)
			{
				isDrifting = false;
			}
		}
		
		private bool AreWeDrifting()
		{
			if (axleInfos.Any(x => x.leftWheel.isGrounded || x.rightWheel.isGrounded))
			{
				Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
				float driftValue = Vector3.Dot(velocity.normalized, transform.forward);

				if (velocity.sqrMagnitude > carSettings.slidingMinimumVelocity && Mathf.Abs(driftValue) < carSettings.slidingThreshold)
				{
					return true;
				}

			}
			return false;
		}

		[Server]
		private void ApplyTorque(float acceleration, float steer)
		{
			// WORK IN PROGRESS 3.0!
			float localForwardVelocity = Vector3.Dot(rb.velocity, transform.forward);
			float scaledVelocity = Mathf.Clamp(localForwardVelocity / carSettings.relativeAccelerationTimeFrame, -1, 1);
			// print("Velocity: " + localForwardVelocity);
			// print("ScaledVelocity: " + scaledVelocity);

			float brakeForce = 1f;
			
			if (acceleration > 0 && localForwardVelocity < 0 || acceleration < 0 && localForwardVelocity > 0)
			{
				acceleration = 0;
				
				float velocityRelativeBrakeMultiplier = Mathf.Clamp(carSettings.regularBrakeMaxMultiplier * carSettings.regularBrakeCurve.Evaluate(Mathf.Abs(scaledVelocity)),
														1, carSettings.regularBrakeMaxMultiplier);
				brakeForce *= velocityRelativeBrakeMultiplier;
				// print("BrakeForce: " + brakeForce);

				// OLD FUNCTION - Not using AnimationCurve
				// brakeForce = carSettings.regularBrakeMultiplier;
			}
			else
			{
				// OLD FUNCTION - Not using AnimationCurve
				// float velocityRelativeMultiplier = Mathf.Clamp(acceleration * carSettings.maxRelativeAccelerationTimeFrame 
				// 													/ localForwardVelocity, 1, carSettings.maxRelativeAccelerationMultiplier);
				
				float velocityRelativeAccelerationMultiplier = Mathf.Clamp(Mathf.Abs(acceleration) / carSettings.accelerationCurve.Evaluate(Mathf.Abs(scaledVelocity)),
																1, carSettings.relativeAccelerationMaxMultiplier);
				acceleration *= velocityRelativeAccelerationMultiplier;
			}
			
			float motor = maxMotorTorque * acceleration;
			// print("Motor: " + motor);

			float steering = carSettings.maxSteeringAngle * steer;

			foreach (CarAxle axleInfo in axleInfos)
			{
				if (axleInfo.hasSteering)
				{
					axleInfo.leftWheel.steerAngle = steering;
					axleInfo.rightWheel.steerAngle = steering;
				}

				if (axleInfo.hasMotor)
				{
					if (motor == 0)
					{
						axleInfo.leftWheel.brakeTorque = carSettings.decelerationForce * brakeForce;
						axleInfo.rightWheel.brakeTorque = carSettings.decelerationForce * brakeForce;
					}
					else if (rb.velocity.sqrMagnitude > maxVelocity)
					{
						axleInfo.leftWheel.brakeTorque = 0;
						axleInfo.rightWheel.brakeTorque = 0;
						axleInfo.leftWheel.motorTorque = 0;
						axleInfo.rightWheel.motorTorque = 0;
					}
					else
					{
						axleInfo.leftWheel.brakeTorque = 0;
						axleInfo.rightWheel.brakeTorque = 0;
						axleInfo.leftWheel.motorTorque = motor;
						axleInfo.rightWheel.motorTorque = motor;
					}
				}

				AutoStabilize(axleInfo);
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

			float antiRollForce = (travelL - travelR) * carSettings.antiRoll;

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
				CmdBoost(false);
				return;
			}
			
			CmdBoost(true);
		}

		[Client]
		private void Boost_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
		{
			CmdBoost(false);
		}
		
		[Command]
		private void CmdBoost(bool turnOn)
		{
			Boost(turnOn);
		}

		[Server]
		private void Boost(bool turnOn)
		{
			if (boostCounterRoutine != null)
			{
				StopCoroutine(boostCounterRoutine);
			}
			boostCounterRoutine = StartCoroutine(BoostRoutine(turnOn));
			
			if (turnOn)
			{
				maxMotorTorque = defaultMaxMotorTorque * carSettings.boostMultiplier;
				maxVelocity = carSettings.maxVelocityBoost;
			}
			else
			{
				maxMotorTorque = defaultMaxMotorTorque;
				maxVelocity = carSettings.maxVelocity;
			}

			RpcToggleParticle(turnOn);
		}

		[ClientRpc]
		private void RpcToggleParticle(bool turnOn)
		{
			if (turnOn)
			{
				boostParticleLeft.Play();
				boostParticleRight.Play();
			}
			else
			{
				boostParticleLeft.Stop();
				boostParticleRight.Stop();
			}
		}

		private void OnDrawGizmos()
		{
			if(carSettings == null)
				return;
			
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position + transform.rotation * carSettings.centerOfMassOffset, 0.05f);
		}

		[Server]
		private IEnumerator BoostRoutine(bool turnOn)
		{
			if (turnOn)
			{
				while (remainingBoost > 0)
				{
					remainingBoost -= Time.deltaTime;
					ServiceLocator.HudManager.TargetUpdateBoostBar(connectionToClient, remainingBoost / carSettings.boostMaxTime);
					yield return null;
				}
				boostCounterRoutine = null;
				Boost(false);
			}
			else
			{
				while (remainingBoost < carSettings.boostMaxTime)
				{
					remainingBoost += Time.deltaTime * carSettings.boostRecoveryRatePerSecond;
					ServiceLocator.HudManager.TargetUpdateBoostBar(connectionToClient, remainingBoost / carSettings.boostMaxTime);
					yield return null;
				}
				boostCounterRoutine = null;
			}
		}
	}
}
