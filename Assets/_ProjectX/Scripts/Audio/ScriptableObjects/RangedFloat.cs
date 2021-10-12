using System;

namespace Audio.ScriptableObjects
{
	[Serializable]
	public struct RangedFloat
	{
		public float minValue;
		public float maxValue;

		public RangedFloat(float defaultMin, float defaultMax)
		{
			minValue = defaultMin;
			maxValue = defaultMax;
		}
	}
}