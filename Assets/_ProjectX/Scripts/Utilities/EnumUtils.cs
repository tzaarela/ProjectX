using System;

namespace Utilites
{
    public static class EnumUtils
    {
        public static T RandomEnumValue<T>(int startValue)
        {
            var values = Enum.GetValues(typeof(T));

            if (startValue > values.Length)
            {
                startValue = values.Length;
            }
            
            int random = UnityEngine.Random.Range(startValue, values.Length - 1);
            return (T)values.GetValue(random);
        }
    }
}