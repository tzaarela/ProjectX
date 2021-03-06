using UnityEngine;

namespace Data.ScriptableObjects
{
	[CreateAssetMenu(fileName = "CarMaterials", menuName = "CarMaterials", order = 0)]
	public class SO_CarMaterials : ScriptableObject
	{
		public Material[] materials;

		public Color[] colors;

		public Color[] secondaryColors;

		public Material GetMaterial(int index)
		{
			return materials[index];
		}
		
		public Color GetColor(int index)
		{
			return colors[index];
		}
		
		public Color GetSecondaryColor(int index)
		{
			return secondaryColors[index];
		}
	}
}