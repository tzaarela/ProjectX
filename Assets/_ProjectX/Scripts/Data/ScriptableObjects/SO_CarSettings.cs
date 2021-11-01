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
		public float wheelMass = 80;
		public float wheelRadius = 0.37f;
		public float wheelDampingRate = 0.0001f;
		public float wheelSuspensionDistance = 0.3f;
		public float wheelForceAppPointDistance;
		[Space(5)]
		public float wheelSuspensionSpring = 35000;
		public float wheelSuspensionDamper = 4500;
		public float wheelSuspensionTargetPosition = 0.5f;
		
		[Header("Other")] 
		public float maxMotorTorque = 700;
		public float maxSteeringAngle = 32;
		public float brakeTorque = 6000;
		public float decelerationForce = 2000;
		public float boostMultiplier = 2.5f;
		public float antiRoll = 16000;
		public Vector3 centerOfMassOffset = new Vector3(0,0.4f,1);

		[Header("Rigidbody")] 
		public float mass = 1300;
		public float drag;
		public float angularDrag;
		public bool useGravity = true;
		public bool isKinematic;
		public RigidbodyInterpolation interpolateMode = RigidbodyInterpolation.Interpolate;
		public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;
	}
}