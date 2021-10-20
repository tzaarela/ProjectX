using Mirror;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;
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
			rb = GetComponent<Rigidbody>();
		}

		public override void OnStartClient()
		{
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

		[ContextMenu("Drop")]
		[Server]
		public void DropFlag()
		{
			hasFlag = false;
			flag.Drop(transform.position, rb.velocity);
		}

		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
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
	}
}