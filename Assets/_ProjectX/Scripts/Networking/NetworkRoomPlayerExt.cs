using UnityEngine;
using Mirror;
using System;
using Managers;
using TMPro;
using System.Collections.Generic;
using System.Linq;
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
	[SerializeField] private Image readyImage;
	[SerializeField] private TextMeshProUGUI readyText;
    
	[Header("Mesh")]
	[SerializeField] private MeshRenderer colorChangingMesh;

	[Header("SyncVars")]
	[HideInInspector]
	[SyncVar(hook = nameof(PlayerColorChanged))]
	public Color playerColor;

	[HideInInspector]
	[SyncVar(hook = nameof(PlayerReadyColorChanged))]
	public Color playerReadyColor;

	// [HideInInspector]
	[SyncVar(hook = nameof(PlayerNameChanged))]
	public string playerName;

	[HideInInspector]
	[SyncVar(hook = nameof(PlayerReadyTextChanged))]
	public string playerReadyText;
    
	private static string[] playerNames = Array.Empty<string>();
	private int playerNamesIndex;

	/// <summary>
	/// Called when the local player object has been set up.
	/// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
	/// </summary>
	public override void OnStartLocalPlayer() {

		LobbyManager lobbyManager = ServiceLocator.LobbyManager;
		List<Color> indexColors = lobbyManager.indexColors;
		Image[] images = lobbyManager.colorPalette.GetComponentsInChildren<Image>();

		for (int i = 0; i < indexColors.Count; i++)
		{
			images[i].color = indexColors[i];
		}

		lobbyManager.lobbyUI.gameObject.SetActive(true);

		CmdMoveToNextSlot(gameObject);
	}

	public override void OnStartServer()
	{
		playerName = "Player" + NetworkServer.connections.Count;
		playerColor = ServiceLocator.LobbyManager.indexColors.ElementAt(NetworkServer.connections.Count - 1);

		if (!isServer)
			return;

		playerNamesIndex = playerNames.Length;
		string[] playerNamesUpdate = new string[playerNames.Length + 1];
		for (int i = 0; i < playerNames.Length; i++)
		{
			playerNamesUpdate[i] = playerNames[i];
		}
		playerNamesUpdate[playerNamesIndex] = playerName;
		playerNames = playerNamesUpdate;
	}

	[Command]
	public void CmdRoomPlayerChangeReadyState(bool readyState)
	{
		readyToBegin = readyState;

		playerReadyColor = readyState ? Color.green : Color.red;
		playerReadyText = readyState ? "Ready" : "Not Ready";
		
		NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
		if (room != null)
		{
			room.ReadyStatusChanged();
		}
	}

	[Command]
	public void CmdMoveToNextSlot(GameObject roomPlayer)
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
	public void PlayerColorChanged(Color oldValue, Color newValue)
	{
		colorChangingMesh.material.color = newValue;
	}

	//hook
	[Client]
	private void PlayerReadyTextChanged(string oldValue, string newValue)
	{
		readyText.text = newValue;
	}

	//hook
	[Client]
	private void PlayerReadyColorChanged(Color oldValue, Color newValue)
	{
		readyImage.color = newValue;
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
	public void CmdChangeColor(int colorIndex)
	{
		playerColor = ServiceLocator.LobbyManager.indexColors[colorIndex];
	}
}