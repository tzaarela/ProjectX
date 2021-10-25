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
		[SerializeField] private MeshRenderer colorChangingMesh;
		[SerializeField] private TMPro.TextMeshProUGUI playerNameText;

		//[Header("Player")]
		//[SerializeField] private Material[] carMaterials;

		[Header("Debug")]
		[SyncVar(hook = "FlagStateChanged")] public bool hasFlag;

		private Flag flag;
		private Rigidbody rb;

		private string playerName;
		public string PlayerName { 
			get { return playerName; } 
			set
			{
				playerName = value;
				RpcUpdatePlayerName(PlayerName);
			}
		}

		[ClientRpc]
		private void RpcUpdatePlayerName(string playerName)
		{
			playerNameText.text = playerName;
		}

		private int playerId;
		public int PlayerId => playerId;

		//private static int materialCount = 0;

		[Server]
		public override void OnStartServer()
		{
			rb = GetComponent<Rigidbody>();
			
			GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnStartClient()
		{
			////TEMP SOLUTION TO GET DIFFERENT CAR COLORS IN GAME
			//GetComponentInChildren<Renderer>().material = carMaterials[materialCount];
			//materialCount++;
			//if (materialCount >= carMaterials.Length)
			//	materialCount = 0;
			
			if (!isLocalPlayer)
				return;

			playerId = (int)GetComponent<NetworkIdentity>().netId;
			print("OnStartClient(netId) " + playerId);
			CmdUpdateActivePlayersList(playerId);
			
			SendGlobal(GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME, new GameObjectData(gameObject));

			name += "-local";
		}

		[Server]
		public void ChangeColor(Color color)
		{
			RpcChangeColor(new float[] { color.r, color.g, color.b });
		}

		[ClientRpc]
		public void RpcChangeColor(float[] colorRbg)
		{
			colorChangingMesh.material.color = new Color(colorRbg[0], colorRbg[1], colorRbg[2]);
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