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

		[Header("Debug")]
		[SyncVar(hook = nameof(FlagStateChanged))] public bool hasFlag;
		[SyncVar(hook = nameof(PlayerNameChanged))] public string playerName;
		[SyncVar(hook = nameof(PlayerColorChanged))] public Color playerColor;

		private Flag flag;
		private Rigidbody rb;

		private int playerId;
		public int PlayerId => playerId;

		public bool localPlayer;

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
			CmdUpdateActivePlayersList(playerId);
			
			SendGlobal(GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME, new GameObjectData(gameObject));

			name += "-local";
		}
		
		private void Update()
		{
			if (!isLocalPlayer)
				return;

			// F-Key resets car-rotation (when turned over)
			if (Keyboard.current.fKey.wasPressedThisFrame)
			{
				Vector3 newRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
				transform.rotation = Quaternion.Euler(newRotation);
			}
		}


		[Server]
		public void TakeFlag(Flag flag)
		{
			this.flag = flag;
			hasFlag = true;
			
			//Which to use??
			SendGlobal(GlobalEvent.FLAG_TAKEN, new GameObjectData(gameObject));
			// ServiceLocator.HudManager.UpdateFlagIndicatorTarget(flagHasBeenTaken: true, gameObject);
		}

		[ContextMenu("Drop Flag")]
		[Server]
		public void DropFlag()
		{	
			hasFlag = false;
			flag.Drop(transform.position, rb.velocity);
			
			// Which to use??
			SendGlobal(GlobalEvent.FLAG_DROPPED);
			// ServiceLocator.HudManager.UpdateFlagIndicatorTarget(flagHasBeenTaken: false, null);
		}

		//Hook
		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
		}

		//Hook
		[Client]
		private void PlayerNameChanged(string oldValue, string newValue)
		{
			playerNameText.text = newValue;
		}

		//Hook
		[Client]
		private void PlayerColorChanged(Color oldValue, Color newValue)
		{
			colorChangingMesh.material.color = newValue;
		}

		[Command]
		private void CmdUpdateActivePlayersList(int id)
		{
			playerId = id;
			ServiceLocator.RoundManager.AddActivePlayer(id);
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

			GetComponent<InputManager>().EnableInput();
		}

		[ClientRpc]
		private void RpcSetPlayerEndState()
		{
			GetComponent<DriveController>().enabled = false;
			GetComponent<InputManager>().DisableInput();
			GetComponent<PlayerSound>().StopEmitter();

			if (!isServer)
				return;
			
			rb.velocity = Vector3.zero;
		}
	}
}