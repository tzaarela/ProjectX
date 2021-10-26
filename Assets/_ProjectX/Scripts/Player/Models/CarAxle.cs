using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Player
{
	[Serializable]
	public class CarAxle
	{
		public WheelCollider leftWheel;
		public WheelCollider rightWheel;
		public bool hasMotor;
		public bool hasSteering;
		public bool hasHandbrake;
	}
}
