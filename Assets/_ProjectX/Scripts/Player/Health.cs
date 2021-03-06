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

		[Header("References")]
		[SerializeField] private MeshRenderer decalRenderer;
		[SerializeField] private GameObject[] smokeFX;
		[SerializeField] private Material[] scratchesMaterials;

		public HealthState currentState = HealthState.Great;

		private PlayerController playerController;

		[SyncVar(hook = nameof(OnHealthChanged))] private int currentHealth;

		private bool isDead;

		public bool IsDead
		{
			get => isDead;
			set => isDead = value;
		}

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
			if (isDead)
				return;
			
			currentHealth -= damage;

			if (currentHealth <= 0)
			{
				playerController.DropFlag();
				GameObject attacker = NetworkServer.spawned[(uint)attackerId].gameObject;
				string attackerName = attacker.GetComponent<PlayerController>().playerName;
				ServiceLocator.HudManager.TargetActivateDeathTexts(connectionToClient, attackerName);
				NetworkConnectionToClient attackerConn = attacker.GetComponent<NetworkIdentity>().connectionToClient;
				ServiceLocator.HudManager.TargetActivateKillText(attackerConn, playerController.playerName);
				ServiceLocator.ScoreManager.AddKillScore(attackerName, attacker.GetComponent<PlayerController>().hasFlag);
			}
		}

		//SyncVar Hook
		[Client]
		private void OnHealthChanged(int oldValue, int newValue)
		{
			// print("PlayerDamaged CurrentHealth = " + newValue);
			// print("PlayerDamaged HealthState = " + GetHealthState(newValue));
			
			if (currentState == GetHealthState(newValue))
				return;

			switch (GetHealthState(newValue))
			{
				case HealthState.Great:
					isDead = false;
					currentState = HealthState.Great;
					decalRenderer.material = scratchesMaterials[0];
					smokeFX[0].SetActive(false);
					smokeFX[1].SetActive(false);
					smokeFX[2].SetActive(false);
					break;
				case HealthState.Good:
					currentState = HealthState.Good;
					decalRenderer.material = scratchesMaterials[1];
					smokeFX[0].SetActive(true);
					//Minor damage
					break;
				case HealthState.Ok:
					currentState = HealthState.Ok;
					decalRenderer.material = scratchesMaterials[2];
					smokeFX[0].SetActive(true);
					smokeFX[1].SetActive(true);
					//Medium damage
					break;
				case HealthState.Bad:
					currentState = HealthState.Bad;
					decalRenderer.material = scratchesMaterials[3];
					smokeFX[0].SetActive(true);
					smokeFX[1].SetActive(true);
					smokeFX[2].SetActive(true);
					//Major damage
					break;
				case HealthState.Dead:
					currentState = HealthState.Dead;
					isDead = true;
					playerController.Death();
					break;
			}
		}

		[Client]
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
			if (health > 0)
			{
				return HealthState.Bad;
			}
			
			return HealthState.Dead;
		}

		[Server]
		public void ResetCurrentHealth()
		{
			currentHealth = startingHealth;
		}
		
		[Server]
		public void SetHealthToZero()
		{
			currentHealth = 0;
		}
		
		[Server]
		public void LowerHealthByTwentyFive()
		{
			if (currentHealth <= 0)
				return;
			
			currentHealth -= 25;
		}
	}
}