using Mirror;
using UnityEngine;

namespace Player
{
	public class Health : NetworkBehaviour
	{
		[SerializeField] private int startingHealth = 100;

		[SyncVar]
		private int currentHealth;

		private void Awake()
		{
			currentHealth = startingHealth;
		}

		public void ReceiveDamage(int damage, int attackerId)
		{
			int thisPlayerId = (int)GetComponent<NetworkIdentity>().netId;
			print($"Player_{thisPlayerId} was damaged\n"
						+ $"by Player_{attackerId}! (Damage = {damage})");
		}
	}
}