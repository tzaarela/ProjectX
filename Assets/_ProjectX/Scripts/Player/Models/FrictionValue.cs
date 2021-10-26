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
	public class FrictionValue
	{
		public FrictionType key;
		public Friction value;
	}

	[Serializable]
	public struct Friction
	{
		public float extremumSlip;
		public float extremumValue;
		public float asymptoteSlip;
		public float asymptoteValue;
		public float stiffness;
	}
}
