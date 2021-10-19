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

		public void ReceiveDamage()
		{
			print($"Player_{GetComponent<NetworkIdentity>().netId} has received damage!");
		}
	}
}