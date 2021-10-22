using UnityEngine;
using Mirror;
using System;
using Managers;
using TMPro;
using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI nameTag;
    [SerializeField] private TextMeshProUGUI nameInput;
    [SerializeField] private GameObject inputField;
    [SerializeField] private GameObject colorPicker;

    [SerializeField] Color color;
    [SerializeField] MeshRenderer colorChangingMesh;
    [SerializeField] Transform topUI;


    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() {
        
        inputField.SetActive(true);

        var lobbyManager = ServiceLocator.LobbyManager;
        var indexColors = lobbyManager.indexColors;
        var images = colorPicker.GetComponentsInChildren<Image>();

        for (int i = 0; i < indexColors.Count; i++)
		{
            images[i].color = indexColors[i];
		}

        colorPicker.SetActive(true);

        CmdMoveToNextSlot(gameObject);
    }

    [Command]
    public void CmdMoveToNextSlot(GameObject roomPlayer)
    {
        LobbyManager lobbyManager = ServiceLocator.LobbyManager;
        var networkRoomPlayer = roomPlayer.GetComponent<NetworkRoomPlayer>();
        networkRoomPlayer.gameObject.transform.position = lobbyManager.roomPlayerSpawnSlots[lobbyManager.networkRoomPlayers.Count - 1].position;
        TargetPositionUI(networkRoomPlayer.connectionToClient);
    }

    [TargetRpc]
    private void TargetPositionUI(NetworkConnection target)
	{
        topUI.position = ServiceLocator.LobbyManager.topWorldUISlot.position + Vector3.right * index * 2;
    }

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority"/> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion
    #region Room Client Callbacks

    /// <summary>
    /// This is a hook that is invoked on all player objects when entering the room.
    /// <para>Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called.</para>
    /// </summary>
    public override void OnClientEnterRoom() { }

    /// <summary>
    /// This is a hook that is invoked on all player objects when exiting the room.
    /// </summary>
    public override void OnClientExitRoom() { }

    #endregion
    #region SyncVar Hooks

    /// <summary>
    /// This is a hook that is invoked on clients when the index changes.
    /// </summary>
    /// <param name="oldIndex">The old index value</param>
    /// <param name="newIndex">The new index value</param>
    public override void IndexChanged(int oldIndex, int newIndex) { }

    /// <summary>
    /// This is a hook that is invoked on clients when a RoomPlayer switches between ready or not ready.
    /// <para>This function is called when the a client player calls SendReadyToBeginMessage() or SendNotReadyToBeginMessage().</para>
    /// </summary>
    /// <param name="oldReadyState">The old readyState value</param>
    /// <param name="newReadyState">The new readyState value</param>
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) { }

	/// <summary>
	/// This is a hook.
	/// </summary>
	/// <param name="oldValue"></param>
	/// <param name="newValue"></param>
	public override void PlayerNameChanged(string oldValue, string newValue)
	{
		nameTag.text = newValue;
	}

	[Client]
    public void ChangeName()
	{
        if (isLocalPlayer)
            CmdChangeName(nameInput.text);
	}

    [Command]
    public void CmdChangeName(string name)
	{
        playerName = name;
        nameTag.text = name;
        RpcChangeName(name);
	}

    [ClientRpc]
    private void RpcChangeName(string name)
	{
        playerName = name;
        nameTag.text = name;
    }

    [Client]
    public void ChangeColor(int colorIndex)
	{
        if (isLocalPlayer)
            CmdChangeColor(colorIndex);  
	}

    [Server]
    public Color GetColor()
	{
        return color;
	}

    [Command]
    public void CmdChangeColor(int colorIndex)
	{
        color = ServiceLocator.LobbyManager.indexColors[colorIndex];
        colorChangingMesh.material.color = color;
        RpcChangeColor(colorIndex);
	}

    [ClientRpc]
	private void RpcChangeColor(int colorIndex)
	{
        color = ServiceLocator.LobbyManager.indexColors[colorIndex];
        colorChangingMesh.material.color = color;
    }

	#endregion
	#region Optional UI

	public override void OnGUI()
    {
        base.OnGUI();
    }

    #endregion
}
