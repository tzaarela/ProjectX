using Mirror;
using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using PowerUp.Projectiles;
using UnityEngine;
using System;
using Game.Flag;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal
	{
		[Header("References")]
		[SerializeField] private GameObject flagOnRoof;
		
		[Header("Debug")]
		[SyncVar(hook = "FlagStateChanged")] public bool hasFlag;

		private Flag flag;
		private Rigidbody rb;
		
		private int playerId;
		
		public int PlayerId => playerId;

		[Server]
		public override void OnStartServer()
		{
			if (!isServer)
				return;
			
			rb = GetComponent<Rigidbody>();
		}

		public override void OnStartClient()
		{
			if (!isLocalPlayer)
				return;
			print("OnStartClient(netId as uInt!) " + GetComponent<NetworkIdentity>().netId);
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			print("OnStartClient(netId) " + playerId);
			SendGlobal(GlobalEvent.SET_FOLLOW_TARGET, new GameObjectData(gameObject));
			CmdUpdateActivePlayersList();

			name += "-local";
		}
		
		[Server]
		public void TakeFlag(Flag flag)
		{
			this.flag = flag;
			hasFlag = true;
		}

		[ContextMenu("Drop")]
		[Server]
		public void DropFlag()
		{
			if (!isServer)
				return;
			
			hasFlag = false;
			flag.Drop(transform.position, rb.velocity);
		}

		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
		}

		[Command]
		private void CmdUpdateActivePlayersList()
		{
			ServiceLocator.RoundManager.AddActivePlayer(playerId);
		}
		
		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}