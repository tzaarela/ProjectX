using Data.Enums;
using kcp2k;
using Mirror;
using Mirror.FizzySteam;
using Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
	public class StartupController : MonoBehaviour
	{
		[SerializeField] private GameObject RoomManagerPrefab;
		[SerializeField] private TMPro.TMP_Dropdown dropdownConnection;
		[SerializeField] private TMPro.TextMeshProUGUI connectionText;
		[SerializeField] private TMPro.TextMeshProUGUI connectionEnterText;
		[SerializeField] private TMPro.TextMeshProUGUI statusText;
		[SerializeField] private NetworkRoomManagerExt roomManager;


		[SerializeField] private FizzySteamworks fizzySteamworks;
		[SerializeField] private KcpTransport kcpTransport;

		private ConnectionType connectionType = ConnectionType.IP;

		private void Awake()
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 300;
		}

		private void Start()
		{
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
						connectionEnterText.text = "Enter ip...";
						fizzySteamworks.gameObject.GetComponent<FizzySteamworks>().enabled = false;
						kcpTransport.gameObject.GetComponent<KcpTransport>().enabled = true;
						roomManager.transport = kcpTransport;
						roomManager.gameObject.SetActive(true);
						break;
					}
				case ConnectionType.Steam:
					{
						roomManager.gameObject.SetActive(false);
						connectionEnterText.text = "Enter steamId...";
						kcpTransport.gameObject.GetComponent<KcpTransport>().enabled = false;
						fizzySteamworks.gameObject.GetComponent<FizzySteamworks>().enabled = true;
						roomManager.transport = fizzySteamworks;
						roomManager.gameObject.SetActive(true);
						break;
					}
			}
		}

		public void SetConnectionString()
		{
			roomManager.networkAddress = connectionText.text;
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

		private IEnumerator WaitForTimeout()
		{
			yield return new WaitForSeconds(kcpTransport.Timeout * 0.001f);
			statusText.text = "connection timed out.";
		}
	}
}