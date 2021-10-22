using System;
using Mirror;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;
using Game.Flag;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal
	{
		[Header("References")]
		[SerializeField] private GameObject flagOnRoof;

		[Header("Player")]
		[SerializeField] private Material[] carMaterials;

		[Header("Debug")]
		[SyncVar(hook = "FlagStateChanged")] public bool hasFlag;

		private Flag flag;
		private Rigidbody rb;
		
		private int playerId;
		public int PlayerId => playerId;

		private static int materialCount = 0;

		[Server]
		public override void OnStartServer()
		{
			rb = GetComponent<Rigidbody>();
			
			GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnStartClient()
		{
			//TEMP SOLUTION TO GET DIFFERENT CAR COLORS IN GAME
			GetComponentInChildren<Renderer>().material = carMaterials[materialCount];
			materialCount++;
			if (materialCount >= carMaterials.Length)
				materialCount = 0;
			
			if (!isLocalPlayer)
				return;

			playerId = (int)GetComponent<NetworkIdentity>().netId;
			print("OnStartClient(netId) " + playerId);
			CmdUpdateActivePlayersList(playerId);
			
			SendGlobal(GlobalEvent.SET_FOLLOW_TARGET, new GameObjectData(gameObject));

			name += "-local";
		}

		[Server]
		public void TakeFlag(Flag flag)
		{
			this.flag = flag;
			hasFlag = true;
		}

		[ContextMenu("Drop Flag")]
		[Server]
		public void DropFlag()
		{	
			hasFlag = false;
			// flag.Drop(transform.position, rb.velocity);
		}

		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
			
			if (!newValue)
			{
				flag.Drop(transform.position, rb.velocity);
			}
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
			if (eventState == GlobalEvent.END_GAMESTATE)
			{
				RpcSetPlayerEndState();
			}
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