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
    [SerializeField] MeshRenderer colorChangingMesh;
    [SerializeField] Image readyImage;
    [SerializeField] TextMeshProUGUI readyText;

    /// <summary>
    /// Player color
    /// </summary>
    [SyncVar(hook = nameof(PlayerColorChanged))]
    public Color playerColor;

    /// <summary>
    /// Player name
    /// </summary>
    [SyncVar(hook = nameof(PlayerNameChanged))]
    public string playerName;

    /// <summary>
    /// Player ready text
    /// </summary>
    [SyncVar(hook = nameof(PlayerReadyTextChanged))]
    public string playerReadyText;

    [Client]
	private void PlayerReadyTextChanged(string oldValue, string newValue)
	{
        readyText.text = newValue;
    }

	/// <summary>
	/// Player name
	/// </summary>
	[SyncVar(hook = nameof(PlayerReadyColorChanged))]
    public Color playerReadyColor;

	private void PlayerReadyColorChanged(Color oldValue, Color newValue)
	{
        readyImage.color = newValue;

    }


	/// <summary>
	/// Called when the local player object has been set up.
	/// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
	/// </summary>
	public override void OnStartLocalPlayer() {

        var lobbyManager = ServiceLocator.LobbyManager;
        var indexColors = lobbyManager.indexColors;
        var images = lobbyManager.LobbyUI.gameObject.GetComponentsInChildren<Image>();

        for (int i = 0; i < indexColors.Count; i++)
		{
            images[i].color = indexColors[i];
		}

        lobbyManager.LobbyUI.gameObject.SetActive(true);
        

        CmdMoveToNextSlot(gameObject);
    }

    [Command]
    public void CmdRoomPlayerChangeReadyState(bool readyState)
    {
        readyToBegin = readyState;

        playerReadyColor = Color.green;
        playerReadyText = "Ready";

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
        var networkRoomPlayer = roomPlayer.GetComponent<NetworkRoomPlayer>();
        networkRoomPlayer.gameObject.transform.position = lobbyManager.roomPlayerSpawnSlots[NetworkRoomManager.singleton.numPlayers - 1].position;
    }  

	/// <summary>
	/// This is a hook.
	/// </summary>
	/// <param name="oldValue"></param>
	/// <param name="newValue"></param>
	public void PlayerNameChanged(string oldValue, string newValue)
	{
		nameTag.text = newValue;
	}

    /// <summary>
	/// This is a hook.
	/// </summary>
	/// <param name="oldValue"></param>
	/// <param name="newValue"></param>
	public void PlayerColorChanged(Color oldValue, Color newValue)
    {
        colorChangingMesh.material.color = newValue;
    }

    [Command]
    public void CmdChangeName(string name)
	{
        playerName = name;
	}
   
    [Command]
    public void CmdChangeColor(int colorIndex)
	{
        playerColor = ServiceLocator.LobbyManager.indexColors[colorIndex];
	}

    /// <summary>
    /// Render a UI for the room. Override to provide your own UI
    /// </summary>
    public void OnGUI()
    {
        if (!showRoomGUI)
            return;

        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room)
        {
            if (!room.showRoomGUI)
                return;

            if (!NetworkManager.IsSceneActive(room.RoomScene))
                return;

            DrawPlayerReadyState();
            DrawPlayerReadyButton();
        }
    }

    void DrawPlayerReadyState()
    {
        GUILayout.BeginArea(new Rect(20f + (index * 100), 200f, 90f, 130f));

        if (string.IsNullOrEmpty(playerName))
            GUILayout.Label($"Player [{index + 1}]");
        else
            GUILayout.Label(playerName);

        if (readyToBegin)
            GUILayout.Label("Ready");
        else
            GUILayout.Label("Not Ready");

        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE"))
        {
            // This button only shows on the Host for all players other than the Host
            // Host and Players can't remove themselves (stop the client instead)
            // Host can kick a Player this way.
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }

        GUILayout.EndArea();
    }

    void DrawPlayerReadyButton()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(20f, 300f, 120f, 20f));

            if (readyToBegin)
            {
                if (GUILayout.Button("Cancel"))
                    CmdChangeReadyState(false);
            }
            else
            {
                if (GUILayout.Button("Ready"))
                    CmdChangeReadyState(true);
            }

            GUILayout.EndArea();
        }
    }
}
