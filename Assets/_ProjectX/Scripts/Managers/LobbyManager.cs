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
	public List<NetworkRoomPlayer> networkRoomPlayers;
	public Transform topWorldUISlot;

	private bool hasBeenProvided;

	private void Awake()
	{
		if (!hasBeenProvided)
		{
			print("RoundManager provided to ServiceLocator");
			ServiceLocator.ProvideLobbyManager(this);
			hasBeenProvided = true;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	[Server]
	public void AddRoomPlayer(NetworkRoomPlayer networkRoomPlayer)
	{
		networkRoomPlayers.Add(networkRoomPlayer);
	}
}
