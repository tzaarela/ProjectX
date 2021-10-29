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
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal
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

		private Flag flag;
		private Rigidbody rb;
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
			
			GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnStartClient()
		{
			if (!isLocalPlayer)
				return;

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
			RpcRespawnPlayer();
		}
		
		[ClientRpc]
		private void RpcRespawnPlayer()
		{
			health.ResetCurrentHealth();
			deathSmoke.Stop();
			colorChangingMesh.material.color = playerColor;
			GetComponent<PlayerSound>().PlayEmitter();
			
			if (!isLocalPlayer)
				return;

			inputManager.EnableInput();
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
			Vector3 newRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
			transform.rotation = Quaternion.Euler(newRotation);
		}
	}
}