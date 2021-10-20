using Data.Enums;
using Mirror;
using UnityEngine;

namespace Player
{
	public class Health : NetworkBehaviour
	{
		[SerializeField] private int startingHealth = 100;

		public HealthState currentState = HealthState.Great;

		[SyncVar(hook = nameof(OnHealthChanged))] private int currentHealth;

		[Server]
		public override void OnStartServer()
		{
			currentHealth = startingHealth;
		}

		[Server]
		public void ReceiveDamage(int damage, int attackerId)
		{
			int thisPlayerId = (int)GetComponent<NetworkIdentity>().netId;
			
			print($"Player_{thisPlayerId} was damaged\n"
						+ $"by Player_{attackerId}! (Damage = {damage})");

			currentHealth -= damage;
		}

		//SyncVar Hook
		[Client]
		private void OnHealthChanged(int oldValue, int newValue)
		{
			if (newValue <= 0)
			{
				// ExplosionFX
				// Respawn
				return;
			}

			if (currentState == GetHealthState(newValue))
				return;
			
			switch (GetHealthState(newValue))
			{
				case HealthState.Great:
					currentState = HealthState.Great;
					//No effect?
					break;
				case HealthState.Good:
					currentState = HealthState.Good;
					//Minor damage
					break;
				case HealthState.Ok:
					currentState = HealthState.Ok;
					//Medium damage
					break;
				case HealthState.Bad:
					currentState = HealthState.Bad;
					//Major damage
					break;
			}
		}

		private HealthState GetHealthState(int health)
		{
			if (health > 75)
			{
				return HealthState.Great;
			}
			if (health > 50)
			{
				return HealthState.Good;
			}
			if (health > 25)
			{
				return HealthState.Ok;
			}
			
			return HealthState.Bad;
		}
	}
}