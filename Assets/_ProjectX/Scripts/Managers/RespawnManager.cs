using Managers;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : NetworkBehaviour
{
    [SerializeField] private List<Transform> respawnPositions;

	[Server]
	public override void OnStartServer()
	{
		ServiceLocator.ProvideRespawnManager(this);

		if (respawnPositions.Count == 0)
			Debug.LogError("No respawnposition added to RespawnManager!");
	}

	[Server]
	public void RespawnPlayer(Transform player)
	{
		int randomSpawnIndex = Random.Range(0, respawnPositions.Count);
		player.rotation = respawnPositions[randomSpawnIndex].rotation;
		player.position = respawnPositions[randomSpawnIndex].position;
	}
}
