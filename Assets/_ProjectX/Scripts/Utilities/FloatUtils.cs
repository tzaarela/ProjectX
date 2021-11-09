namespace Utilites
{
	public static class FloatUtils
	{
		public static float Normalize(float value, float minValue, float maxValue)
		{
			return (value - minValue) / (maxValue - minValue);
		}
		
		public static float Denormalize(float normalizedValue, float minValue, float maxValue) 
		{
			return normalizedValue * (maxValue - minValue) + minValue;
		}
	}
}