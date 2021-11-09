using System.Collections.Generic;
using Player;
using UnityEngine;

namespace _ProjectX.Scripts.Data.ScriptableObjects
{
	[CreateAssetMenu(fileName = "CarSettings", menuName = "ScriptableObjects/CarSettings")]
	public class SO_CarSettings : ScriptableObject
	{
		[Header("Front Wheels")]
		public bool f_HasMotor = true;
		public bool f_HasSteering = true;
		public bool f_HasHandbrake;
		[Header("Rear Wheels")]
		public bool r_HasMotor = true;
		public bool r_HasSteering;
		public bool r_HasHandbrake = true;
		
		[Space(10)]
		public List<FrictionCurve> frictionCurves;

		[Header("Wheel Collider")] 
		public float wheelMass = 80f;
		public float wheelRadius = 0.37f;
		public float wheelDampingRate = 0.0001f;
		public float wheelSuspensionDistance = 0.3f;
		public float wheelForceAppPointDistance;
		[Space(5)]
		public float wheelSuspensionSpring = 35000f;
		public float wheelSuspensionDamper = 4500f;
		public float wheelSuspensionTargetPosition = 0.5f;

		[Header("Other")]
		public float maxVelocity = 800f;
		public float relativeAccelerationMaxMultiplier = 3f;
		public float relativeAccelerationTimeFrame = 30f;
		public AnimationCurve accelerationCurve;
		public float maxVelocityBoost = 1200f;
		public float maxMotorTorque = 700f;
		public float maxSteeringAngle = 32f;
		public float handBrakeTorque = 6000f;
		public float decelerationForce = 2000f;
		public float regularBrakeMaxMultiplier = 30f;
		public AnimationCurve regularBrakeCurve;
		public float boostMultiplier = 2.5f;
		public float boostMaxTime = 2f;
		public float boostRecoveryRatePerSecond = 1f;
		public float antiRoll = 16000f;
		
		[Range(0, 1)]
		[Tooltip("This value determines if we are to be considered sliding, when our sliding value goes below this")]
		public float slidingThreshold = 0.85f;
		public float slidingMinimumVelocity = 2f;
		public Vector3 centerOfMassOffset = new Vector3(0,0.4f,1);

		[Header("Rigidbody")] 
		public float mass = 1300f;
		public float drag;
		public float angularDrag;
		public bool useGravity = true;
		public bool isKinematic;
		public RigidbodyInterpolation interpolateMode = RigidbodyInterpolation.Interpolate;
		public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;
	}
}