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
		
		[SerializeField] private int timeLimit = 100;
		
		[SerializeField] private TMP_Text timeText;

		[SyncVar(hook = nameof(UpdateUiTime))] private int uiTime;

		[Server]
		public override void OnStartServer()
		{
			GlobalMediator.Instance.Subscribe(this);
		}

		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			switch (eventState)
			{
				case GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME:
					uiTime = timeLimit;
					RpcSetTimeScale(1);
					StartCoroutine(TimerRoutine());
					break;
				
				case GlobalEvent.END_GAMESTATE:
					RpcSetTimeScale(0);
					break;
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
			
			ServiceLocator.RoundManager.EndOfGame();
		}

		//SyncVar Hook
		[Client]
		private void UpdateUiTime(int oldValue, int newTime)
		{
			timeText.text =  newTime.ToString();
		}
		
		[ClientRpc]
		private void RpcSetTimeScale(float timeScale)
		{
			Time.timeScale = timeScale;
		}
		
		[ServerCallback]
		private void OnDestroy()
		{
			// print("TimeController OnDestroy");
			GlobalMediator.Instance.UnSubscribe(this);
		}
	}
}