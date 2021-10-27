using System.Collections.Generic;
using Player;
using UnityEngine;

namespace _ProjectX.Scripts.Data.ScriptableObjects
{
	[CreateAssetMenu(fileName = "CarSettings", menuName = "ScriptableObjects/CarSettings")]
	public class SO_CarSettings : ScriptableObject
	{
		[Header("Front Wheels")]
		public bool f_HasMotor;
		public bool f_HasSteering;
		public bool f_HasHandbrake;
		[Header("Front Wheels")]
		public bool r_HasMotor;
		public bool r_HasSteering;
		public bool r_HasHandbrake;
		
		[Space(10)]
		public List<FrictionCurve> frictionCurves;
		
		[Header("Other")] 
		public float maxMotorTorque;
		public float maxSteeringAngle;
		public float brakeTorque;
		public float decelerationForce;
		public float boostMultiplier;
		public float antiRoll;
		public Vector3 centerOfMassOffset;

		[Header("Rigidbody")] 
		public float mass;
		public float drag;
		public float angularDrag;
		public bool useGravity;
		public bool isKinematic;
		public RigidbodyInterpolation interpolateMode;
		public CollisionDetectionMode collisionDetectionMode;
	}
}