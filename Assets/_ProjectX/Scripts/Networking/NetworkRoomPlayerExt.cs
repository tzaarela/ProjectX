using UnityEngine;
using Mirror;
using System;
using Managers;
using TMPro;
using Data.ScriptableObjects;
using UnityEngine.UI;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-player
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomPlayer.html
*/

/// <summary>
/// This component works in conjunction with the NetworkRoomManager to make up the multiplayer room system.
/// The RoomPrefab object of the NetworkRoomManager must have this component on it.
/// This component holds basic room player data required for the room to function.
/// Game specific data for room players can be put in other components on the RoomPrefab or in scripts derived from NetworkRoomPlayer.
/// </summary>
public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
	[Header("UI References")]
	[SerializeField] private TextMeshProUGUI nameTag;
	[SerializeField] private TextMeshProUGUI readyText;

	[Header("Mesh")]
	[SerializeField] private MeshRenderer colorChangingMesh;

	[Header("Materials")]
	[SerializeField] private SO_CarMaterials carMaterials;

	[Header("SyncVars")]
	[HideInInspector]
	[SyncVar(hook = nameof(PlayerMaterialChanged))]
	public int playerMaterialIndex;

	[HideInInspector]
	[SyncVar(hook = nameof(PlayerReadyColorChanged))]
	public Color playerReadyColor;

	[HideInInspector]
	[SyncVar(hook = nameof(PlayerNameChanged))]
	public string playerName;

	[HideInInspector]
	[SyncVar(hook = nameof(PlayerReadyTextChanged))]
	public string playerReadyText;
    
	private static string[] playerNames = Array.Empty<string>();
	private int playerNamesIndex;

	[Server]
	public override void OnStartServer()
	{
		int connectionIndex = NetworkServer.connections.Count;
		playerName = "Player" + connectionIndex;
		playerMaterialIndex = connectionIndex - 1;

		playerNamesIndex = playerNames.Length;
		string[] playerNamesUpdate = new string[playerNames.Length + 1];
		for (int i = 0; i < playerNames.Length; i++)
		{
			playerNamesUpdate[i] = playerNames[i];
		}
		playerNamesUpdate[playerNamesIndex] = playerName;
		playerNames = playerNamesUpdate;
	}
	
	public override void OnStartLocalPlayer()
	{
		CmdMoveToNextSlot(gameObject);
	}

	[Command]
	public void CmdRoomPlayerChangeReadyState(bool readyState)
	{
		readyToBegin = readyState;

		playerReadyColor = readyToBegin ? Color.green : Color.red;
		playerReadyText = readyToBegin ? "Ready" : "Not Ready";

		NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
		if (room != null)
		{
			room.ReadyStatusChanged();
		}
	}

	[Command]
	private void CmdMoveToNextSlot(GameObject roomPlayer)
	{
		LobbyManager lobbyManager = ServiceLocator.LobbyManager;
		NetworkRoomPlayer networkRoomPlayer = roomPlayer.GetComponent<NetworkRoomPlayer>();
		networkRoomPlayer.gameObject.transform.position = lobbyManager.roomPlayerSpawnSlots[NetworkRoomManager.singleton.numPlayers - 1].position;
	}

	//hook
	[Client]
	public void PlayerNameChanged(string oldValue, string newValue)
	{
		nameTag.text = newValue;
	}

	//hook
	[Client]
	private void PlayerMaterialChanged(int oldValue, int newValue)
	{
		colorChangingMesh.material = carMaterials.GetMaterial(newValue);
	}

	//hook
	[Client]
	private void PlayerReadyTextChanged(string oldValue, string newValue)
	{
		readyText.text = newValue;
		
		if (newValue == "Ready")
		{
			FMODUnity.RuntimeManager.PlayOneShot("event:/UI/PlayerReady", Camera.main.transform.position);
		}
	}

	//hook
	[Client]
	private void PlayerReadyColorChanged(Color oldValue, Color newValue)
	{
		readyText.color = newValue;
	}

	[Client]
	public void UpdateNameTagText(string newText)
	{
		nameTag.text = newText;
	}

	[Command]
	public void CmdChangeName(string name)
	{
		for (int i = 0; i < playerNames.Length; i++)
		{
			if (i == playerNamesIndex)
				continue;

			if (string.Equals(playerNames[i], name, StringComparison.OrdinalIgnoreCase))
			{
				name += $"_{playerNamesIndex.ToString()}";
			}
		}
		playerNames[playerNamesIndex] = name;
		playerName = name;
	}
   
	[Command]
	public void CmdChangeColor(int index)
	{
		playerMaterialIndex = index;
	}

	private void OnDestroy()
	{
		if (!isServer)
			return;
		
		playerNames = Array.Empty<string>();
	}
}