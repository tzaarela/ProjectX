using Data.Enums;
using kcp2k;
using Mirror;
using Mirror.FizzySteam;
using Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
	public class StartupController : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private TMPro.TMP_Dropdown dropdownConnection;
		[SerializeField] private TMPro.TMP_InputField connectionInputField;
		[SerializeField] private TMPro.TextMeshProUGUI connectionPlaceholderText;
		[SerializeField] private TMPro.TextMeshProUGUI statusText;

		[Header("Settings")]
		public int frameRate = 300;
		
		private NetworkRoomManagerExt roomManager;
		private FizzySteamworks fizzySteamworks;
		private KcpTransport kcpTransport;
		
		private ConnectionType connectionType = ConnectionType.IP;

		private void Awake()
		{

			string[] arguments = Environment.GetCommandLineArgs();
			if (arguments.Length == 2)
			{
				frameRate = Convert.ToInt32(arguments[1]);
			}

			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = frameRate;
		}

		private void Start()
		{
			// Solves issue with losing serialized references when returning to MainMenu:
			GameObject roomManagerGO = GameObject.Find("NetworkRoomManagerExt");
			roomManager = roomManagerGO.GetComponent<NetworkRoomManagerExt>();
			fizzySteamworks = roomManagerGO.GetComponent<FizzySteamworks>();
			kcpTransport = roomManagerGO.GetComponent<KcpTransport>();

			SetConnectionMode();
		}

		public void SetConnectionMode()
		{
			this.connectionType = (ConnectionType)dropdownConnection.value;
			
			switch (connectionType)
			{
				case ConnectionType.IP:
					{
						roomManager.gameObject.SetActive(false);
						connectionPlaceholderText.text = "Enter ip...";
						fizzySteamworks.gameObject.GetComponent<FizzySteamworks>().enabled = false;
						kcpTransport.gameObject.GetComponent<KcpTransport>().enabled = true;
						Transport.activeTransport = kcpTransport;
						roomManager.gameObject.SetActive(true);
						break;
					}
				case ConnectionType.Steam:
					{
						roomManager.gameObject.SetActive(false);
						connectionPlaceholderText.text = "Enter steamId...";
						kcpTransport.gameObject.GetComponent<KcpTransport>().enabled = false;
						fizzySteamworks.gameObject.GetComponent<FizzySteamworks>().enabled = true;
						Transport.activeTransport = fizzySteamworks;
						roomManager.gameObject.SetActive(true);
						break;
					}
			}

			if (NetworkRoomManagerExt.serverAdress != "localhost")
			{
				connectionInputField.text = NetworkRoomManagerExt.serverAdress;
				roomManager.networkAddress = NetworkRoomManagerExt.serverAdress;
			}
		}

		public void SetConnectionString()
		{
			NetworkRoomManagerExt.serverAdress = connectionInputField.text;
			roomManager.networkAddress = connectionInputField.text;
		}
		
		// TEMP. for QuickButtons:
		public void SetConnectionString(string steamId)
		{
			if (connectionType == ConnectionType.IP)
			{
				connectionType = ConnectionType.Steam;
				roomManager.gameObject.SetActive(false);
				connectionPlaceholderText.text = "Enter steamId...";
				kcpTransport.gameObject.GetComponent<KcpTransport>().enabled = false;
				fizzySteamworks.gameObject.GetComponent<FizzySteamworks>().enabled = true;
				Transport.activeTransport = fizzySteamworks;
				roomManager.gameObject.SetActive(true);
			}
			
			roomManager.networkAddress = steamId;
			
			JoinGame();
		}

		public void HostGame()
		{
			if (roomManager == null)
				Debug.LogError("roomManager not found when starting host");

			roomManager.StartHost();
		}
		
		public void JoinGame()
		{
			
			if (roomManager == null)
				Debug.LogError("roomManager not found when starting client");

			Debug.Log("trying to join game...");
			if (connectionType == ConnectionType.IP)
			{
				statusText.text = "connecting...";
				StartCoroutine(WaitForTimeout());
			}
			roomManager.StartClient();

		}

		public void QuitGame()
		{
			Application.Quit();
		}

		private IEnumerator WaitForTimeout()
		{
			yield return new WaitForSeconds(kcpTransport.Timeout * 0.001f);
			statusText.text = "connection timed out.";
		}
	}
}