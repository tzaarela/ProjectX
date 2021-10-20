using System.Collections;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using Mirror;
using TMPro;
using UnityEngine;

namespace UI
{
	public class TimeController : NetworkBehaviour, IReceiveGlobalSignal
	{
		// NetworkIdentity = !ServerOnly
		
		[SerializeField] private float timeLimit = 100f;
		
		[SerializeField] private TMP_Text timeText;

		[SyncVar(hook = nameof(UpdateUiTime))] private int uiTime;

		[Server]
		public override void OnStartServer()
		{
			if (!isServer)
				return;
			
			GlobalMediator.Instance.Subscribe(this);
		}
		
		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			if (eventState == GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME)
			{
				uiTime = (int)timeLimit;
				StartCoroutine(TimerRoutine());
			}
		}

		[Server]
		private IEnumerator TimerRoutine()
		{
			while (uiTime > 0)
			{
				yield return new WaitForSeconds(1f);
				uiTime--;
				yield return null;
			}
		}

		//SyncVar Hook
		[Client]
		private void UpdateUiTime(int oldValue, int newTime)
		{
			timeText.text =  newTime.ToString();
		}
	}
}