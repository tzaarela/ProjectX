using System;
using Managers;
using Mirror;
using System.Collections.Generic;
using Data.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Networking;

public class LobbyManager : MonoBehaviour
{
	public List<Transform> roomPlayerSpawnSlots;
	public GameObject colorPalette;
	public int nameCharacterLimit = 10;
	public Image readyButtonImage;
	public TextMeshProUGUI readyButtonText;
	public GameObject StartGameButton;

	[SerializeField] private TMP_InputField textInput;
	[SerializeField] private SO_CarMaterials carMaterials;
	[SerializeField] private GameObject gameSettings;
	[SerializeField] private TMP_Dropdown levelSelectDropdown;

	//These have to be in the same order as the dropdown
	private string[] levels = new string[]
	{
		"City",
		"BlockFort"
	};

	private bool playerIsReady;
	
	private void Awake()
	{
		ServiceLocator.ProvideLobbyManager(this);
	}

	private void Start()
	{
		Image[] images = colorPalette.GetComponentsInChildren<Image>();
		for (int i = 0; i < carMaterials.colors.Length; i++)
		{
			images[i].color = carMaterials.colors[i];
		}

		if (NetworkServer.active)
		{
			gameSettings.SetActive(true);
		}
	}

	[Server]
	public void SelectLevelIndex()
	{
		NetworkRoomManagerExt.singleton.gameObject.GetComponent<NetworkRoomManagerExt>().GameplayScene = levels[levelSelectDropdown.value];
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
