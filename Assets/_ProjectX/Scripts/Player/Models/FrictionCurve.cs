using Data.Enums;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player
{
	[Serializable]
	public class FrictionCurve
	{
		public FrictionType key;
		public SidewayFriction sidewayFriction;
		public ForwardFriction forwardFriction;
	}

	[Serializable]
	public struct SidewayFriction
	{
		public float extremumSlip;
		public float extremumValue;
		public float asymptoteSlip;
		public float asymptoteValue;
		public float stiffness;
	}

	[Serializable]
	public struct ForwardFriction
	{
		public float extremumSlip;
		public float extremumValue;
		public float asymptoteSlip;
		public float asymptoteValue;
		public float stiffness;
	}
}
