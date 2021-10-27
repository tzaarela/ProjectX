using Managers;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	public List<Transform> roomPlayerSpawnSlots;
    public List<Color> indexColors;
	public Transform LobbyUI;

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
	
	[Client]
	public void ChangeColor(int colorIndex)
	{
		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdChangeColor(colorIndex);
	}

	[Client]
	public void ChangeName() 
	{
		if(nameText.text.Length < 3)
		{
			Debug.Log("name needs to be atleast 2 characters.");
			return;
		}	

		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdChangeName(nameText.text);
	}

	[Client]
	public void ReadyUp()
	{
		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdRoomPlayerChangeReadyState(true);

	}
}
