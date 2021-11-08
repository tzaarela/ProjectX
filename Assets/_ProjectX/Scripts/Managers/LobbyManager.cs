using Managers;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	public List<Transform> roomPlayerSpawnSlots;
    public List<Color> indexColors;
	public Transform LobbyUI;
	public int nameCharacterLimit = 10;
	
	[SerializeField] private TMP_InputField textInput;
	
	public void Awake()
	{
		print("LobbyManager provided to ServiceLocator");
		ServiceLocator.ProvideLobbyManager(this);
	}

	[Client]
	public void ChangeColor(int colorIndex)
	{
		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdChangeColor(colorIndex);
	}

	[Client]
	public void UpdateNameTag(string newText)
	{
		if (newText.Length > nameCharacterLimit)
			return;
		
		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.UpdateNameTagText(newText);
	}

	[Client]
	public void ChangeName() 
	{
		if(textInput.text.Length < 3)
		{
			Debug.Log("Name needs to be at least 2 characters.");
			textInput.text = "Mr.2Short";
		}	

		if(textInput.text.Length > nameCharacterLimit)
		{
			textInput.text = textInput.text.Substring(0, nameCharacterLimit);
		}

		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdChangeName(textInput.text);
	}

	[Client]
	public void ReadyUp()
	{
		var roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdRoomPlayerChangeReadyState(true);
	}
	
	private void OnDestroy()
	{
		ServiceLocator.ProvideLobbyManager(null);
	}
}
