using Data.Enums;
using Managers;
using Mirror;
using UnityEngine;

namespace Player
{
	public class Health : NetworkBehaviour
	{
		[Header("Settings")]
		[SerializeField] private int startingHealth = 100;

		[SerializeField] private GameObject[] smokeFX;

		public HealthState currentState = HealthState.Great;

		private PlayerController playerController;

		[SyncVar(hook = nameof(OnHealthChanged))] private int currentHealth;

		private void Awake()
		{
			playerController = GetComponent<PlayerController>();
		}

		[Server]
		public override void OnStartServer()
		{
			currentHealth = startingHealth;
		}

		[Server]
		public void ReceiveDamage(int damage, int attackerId)
		{
			int thisPlayerId = (int)GetComponent<NetworkIdentity>().netId;
			
			print($"Player_{thisPlayerId} was damaged by Player_{attackerId}! (Damage = {damage})");

			currentHealth -= damage;
			print($"Player_{thisPlayerId} CurrentHealth: " + currentHealth);

			if (currentHealth <= 0)
			{
				ServiceLocator.HudManager.TargetActivateDeathTexts(connectionToClient, attackerId);
			}
		}

		//SyncVar Hook
		[Client]
		private void OnHealthChanged(int oldValue, int newValue)
		{
			print("PlayerDamaged CurrentHealth = " + newValue);
			print("PlayerDamaged HealthState = " + GetHealthState(newValue));
			
			if (newValue <= 0)
			{
				print("Player Destroyed!");
				playerController.Death();
				// Respawn
				// currentHealth = 100;
				// currentState = HealthState.Great;
				// smokeFX[0].SetActive(false);
				// smokeFX[1].SetActive(false);
				// smokeFX[2].SetActive(false);
				return;
			}

			if (currentState == GetHealthState(newValue))
				return;

			switch (GetHealthState(newValue))
			{
				case HealthState.Great:
					currentState = HealthState.Great;
					smokeFX[0].SetActive(false);
					smokeFX[1].SetActive(false);
					smokeFX[2].SetActive(false);
					break;
				case HealthState.Good:
					currentState = HealthState.Good;
					smokeFX[0].SetActive(true);
					//Minor damage
					break;
				case HealthState.Ok:
					currentState = HealthState.Ok;
					smokeFX[0].SetActive(true);
					smokeFX[1].SetActive(true);
					//Medium damage
					break;
				case HealthState.Bad:
					currentState = HealthState.Bad;
					smokeFX[0].SetActive(true);
					smokeFX[1].SetActive(true);
					smokeFX[2].SetActive(true);
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