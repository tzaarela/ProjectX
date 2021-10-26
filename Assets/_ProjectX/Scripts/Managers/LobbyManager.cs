using Managers;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

	public List<NetworkRoomPlayerExt> networkRoomPlayers;
    
	public List<Transform> roomPlayerSpawnSlots;
    public List<Color> indexColors;
	public Transform LobbyUI;

	public Action onNameChanged;
	public Action onColorChanged;

	[SerializeField] private TMPro.TextMeshProUGUI nameText;
	private bool hasBeenProvided;

	public void Awake()
	{
		if (!hasBeenProvided)
		{
			ServiceLocator.ProvideLobbyManager(this);
			print("LobbyManager provided to ServiceLocator");
			hasBeenProvided = true;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	[Server]
	public void AddRoomPlayer(NetworkRoomPlayerExt networkRoomPlayer)
	{
		networkRoomPlayers.Add(networkRoomPlayer);
	}
	
	[Client]
	public void ChangeColor(int colorIndex)
	{
		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdChangeColor(colorIndex);
	}

	[Client]
	public void ChangeName() 
	{
		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdChangeName(nameText.text);
	}
}
