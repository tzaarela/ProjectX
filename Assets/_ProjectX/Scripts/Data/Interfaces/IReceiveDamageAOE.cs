using UnityEngine;

namespace Data.Interfaces
{
	public interface IReceiveDamageAOE
	{
		public void ReceiveDamageAOE(Vector3 direction, float distance, int damage, int spawnedById);
	}
}