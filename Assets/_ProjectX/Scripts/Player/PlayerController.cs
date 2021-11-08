using System;
using Mirror;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;
using Game.Flag;
using UnityEngine.InputSystem;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal, IReceiveDamageAOE
	{
		[Header("References")]
		[SerializeField] private GameObject flagOnRoof;
		[SerializeField] private MeshRenderer colorChangingMesh;
		[SerializeField] private TMPro.TextMeshProUGUI playerNameText;

		[Header("DeathSettings")] 
		[SerializeField] private ParticleSystem deathFX;
		[SerializeField] private ParticleSystem deathFX2;
		[SerializeField] private ParticleSystem deathSmoke;
		
		[Header("Debug")]
		[SyncVar(hook = nameof(FlagStateChanged))] public bool hasFlag;
		[SyncVar(hook = nameof(PlayerNameChanged))] public string playerName;
		[SyncVar(hook = nameof(PlayerColorChanged))] public Color playerColor;

		[HideInInspector] public Rigidbody rb;
		private Flag flag;
		private InputManager inputManager;
		private Health health;

		private int playerId;
		public int PlayerId => playerId;

		public bool localPlayer;

		private void Awake()
		{
			inputManager = GetComponent<InputManager>();
			health = GetComponent<Health>();
		}

		[Server]
		public override void OnStartServer()
		{
			rb = GetComponent<Rigidbody>();
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnStartClient()
		{
			if (!isLocalPlayer)
				return;

			rb = GetComponent<Rigidbody>();
			localPlayer = true;
			playerNameText.gameObject.SetActive(false);
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			print("OnStartClient(netId) " + playerId);
			CmdUpdateActivePlayersList();
			name += "-local";
			SendGlobal(GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME, new GameObjectData(gameObject));
		}
		
		private void Update()
		{
			if (!isLocalPlayer)
				return;

			// DEBUG: F-Key resets car-rotation (when turned over)
			if (Keyboard.current.fKey.wasPressedThisFrame)
			{
				CmdFlipCar();
			}
			
			// DEBUG: I-Key = InstantDeath
			if (Keyboard.current.iKey.wasPressedThisFrame)
			{
				CmdDeath();
			}
		}

		[Server]
		public void TakeFlag(Flag flag)
		{
			this.flag = flag;
			hasFlag = true;
			ServiceLocator.HudManager.UpdateFlagIndicatorTarget(flagHasBeenTaken: true, gameObject);
		}

		[ContextMenu("Drop Flag")]
		[Server]
		public void DropFlag()
		{
			if (!hasFlag)
				return;

			hasFlag = false;
			flag.Drop(transform.position, rb.velocity);
			ServiceLocator.HudManager.UpdateFlagIndicatorTarget(flagHasBeenTaken: false, null);
		}

		//SyncVar Hook
		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
		}

		//SyncVar Hook
		[Client]
		private void PlayerNameChanged(string oldValue, string newValue)
		{
			playerNameText.text = newValue;
		}

		//SyncVar Hook
		[Client]
		private void PlayerColorChanged(Color oldValue, Color newValue)
		{
			colorChangingMesh.material.color = newValue;
		}

		[Command]
		private void CmdUpdateActivePlayersList()
		{
			ServiceLocator.RoundManager.AddActivePlayer(playerName);
		}

		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}

		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			switch (eventState)
			{
				case GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME:
					RpcEnableAllPlayersInput();
					break;
				
				case GlobalEvent.END_GAMESTATE:
					RpcSetPlayerEndState();
					break;
			}
		}

		[ClientRpc]
		private void RpcEnableAllPlayersInput()
		{
			if (!isLocalPlayer)
				return;

			inputManager.EnableInput();
		}

		[ClientRpc]
		private void RpcSetPlayerEndState()
		{
			inputManager.DisableInput();
			GetComponent<DriveController>().enabled = false;
			GetComponent<PlayerSound>().StopEmitter();

			if (!isServer)
				return;
			
			rb.velocity = Vector3.zero;
		}

		[Client]
		public void Death()
		{
			FMODUnity.RuntimeManager.PlayOneShot("event:/Vehicles/DeathExplosion", Camera.main.transform.position);
			deathFX.Play();
			deathFX2.Play();
			deathSmoke.Play();
			colorChangingMesh.material.color = Color.black;
			GetComponent<PlayerSound>().StopEmitter();
			
			if (!isLocalPlayer)
				return;
			
			inputManager.DisableInput();
		}

		[Command]
		public void CmdRespawnPlayer()
		{
			health.ResetCurrentHealth();
			RpcRespawnPlayer();
		}
		
		[ClientRpc]
		private void RpcRespawnPlayer()
		{
			deathSmoke.Stop();
			colorChangingMesh.material.color = playerColor;
			GetComponent<PlayerSound>().PlayEmitter();
			FlipCar();
			
			if (!isLocalPlayer)
				return;

			inputManager.EnableInput();
		}
		
		[Client]
		private void FlipCar()
		{
			Vector3 newRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
			transform.rotation = Quaternion.Euler(newRotation);
		}

		// TEMP!
		[Command]
		private void CmdDeath()
		{
			RpcDeath();
			DropFlag();
			ServiceLocator.HudManager.TargetActivateDeathTexts(connectionToClient, "Mr.Debug");
		}

		// TEMP!
		[ClientRpc]
		private void RpcDeath()
		{
			health.IsDead = true;
			Death();
		}

		// TEMP!
		[Command]
		private void CmdFlipCar()
		{
			RpcFlipCar();
		}

		// TEMP!
		[ClientRpc]
		private void RpcFlipCar()
		{
			FlipCar();
		}

		public void ReceiveDamageAOE(Vector3 direction, float distance, int damage, int spawnedById)
		{
			rb.AddForce(direction * (distance * 100) + Vector3.up * 10000, ForceMode.Impulse);
			health.ReceiveDamage(50, spawnedById);
		}
		
		private void OnCollisionEnter(Collision other)
		{
			if (!isServer)
				return;
			
			if (other.transform.CompareTag("Obstacle"))
			{
				// if (rb.velocity.magnitude > 0.1f)
				// 	return;
				
				//Vector3.Dot(other.contacts[0].normal,other.relativeVelocity) * rb.mass

				if (other.impulse.magnitude < 500f)
					return;
				
				Debug.Log("FORCE MAG: " + other.impulse.magnitude);

				Vector3 pushForce = Vector3.ClampMagnitude(other.impulse, 12000);
				
				ForcePush(pushForce);
			}
		}

		[Server]
		private void ForcePush(Vector3 force)
		{
			Debug.Log("FORCE PUSH: " + force);
			Debug.DrawRay(transform.position, force, Color.red, 0.1f);
			rb.AddForce(force, ForceMode.Impulse);
		}
	}
}