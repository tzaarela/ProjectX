using Managers;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
	public List<Transform> roomPlayerSpawnSlots;
    public List<Color> indexColors;
	public Transform lobbyUI;
	public GameObject colorPalette;
	public int nameCharacterLimit = 10;
	public Image readyButtonImage;
	public TextMeshProUGUI readyButtonText;

	[SerializeField] private TMP_InputField textInput;

	private bool playerIsReady;
	
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
			textInput.text = "Too Short";
		}	

		if(textInput.text.Length > nameCharacterLimit)
		{
			textInput.text = textInput.text.Substring(0, nameCharacterLimit);
		}

		NetworkRoomPlayerExt roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdChangeName(textInput.text);
	}

	[Client]
	public void ReadyUp()
	{
		playerIsReady = !playerIsReady;
		
		readyButtonImage.color = playerIsReady ? Color.red : Color.green;
		readyButtonText.text = playerIsReady ? "Not Ready" : "Ready";
		
		Button[] colorButtons = colorPalette.GetComponentsInChildren<Button>();
		foreach (Button button in colorButtons)
		{
			button.interactable = !playerIsReady;
		}

		textInput.interactable = !playerIsReady;

		NetworkRoomPlayerExt roomPlayer = NetworkClient.connection.identity.gameObject.GetComponent<NetworkRoomPlayerExt>();
		roomPlayer.CmdRoomPlayerChangeReadyState(playerIsReady);
	}
	
	private void OnDestroy()
	{
		ServiceLocator.ProvideLobbyManager(null);
	}
}
