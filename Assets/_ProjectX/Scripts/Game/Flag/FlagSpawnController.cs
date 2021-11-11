using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Flag
{
	public class FlagSpawnController : NetworkBehaviour
	{

		public float timeUntilFlagResetStart = 10f;
		public float resetTime = 3f;

		[SerializeField] private Flag flag;
		[SerializeField] private Transform flagSlot;
		[SerializeField] private GameObject flagPresentEffect;
		[SerializeField] private GameObject flagNotPresentEffect;

		private bool flagPresent = true;
		private Coroutine flagResetCouroutine;

		private void Start()
		{
			if (!isServer)
				return;

			if (flag == null)
				Debug.LogWarning("Forgot to set flag reference on flagSpawn");

			flag.onFlagPickedUp += HandleOnFlagPickedUp;
			flag.onFlagDropped += HandleOnFlagDropped;
			flag.StartRotating();
			
			
		}


		[Server]
		private void HandleOnFlagPickedUp()
		{
			if (flagPresent)
			{
				flag.StopRotating();
				flagPresent = false;
				RpcFlagNotPresentEffect();
			}
			else if (flagResetCouroutine != null)
				StopCoroutine(flagResetCouroutine);

		}

		[ClientRpc]
		private void RpcFlagPresentEffect()
		{
			flagNotPresentEffect.SetActive(false);
			flagPresentEffect.SetActive(true);
		}

		[ClientRpc]
		private void RpcFlagNotPresentEffect()
		{
			flagNotPresentEffect.SetActive(true);
			flagPresentEffect.SetActive(false);
		}

		[Server]
		private void HandleOnFlagDropped()
		{
			flagResetCouroutine = StartCoroutine(CoWaitForResetTimer());
			flag.RpcActivateFlag();
		}

		private IEnumerator CoWaitForResetTimer()
		{
			yield return new WaitForSeconds(timeUntilFlagResetStart);
				flag.RpcStartResetBlinking(resetTime);
			yield return new WaitForSeconds(resetTime);
				RespawnFlag();
		}

		[Server]
		public void RespawnFlag()
		{
			flag.RpcDeactivateFlag();
			flag.GetComponent<Rigidbody>().isKinematic = true;
			flag.transform.rotation = Quaternion.identity;
			flag.transform.position = flagSlot.position;
			flag.RpcActivateFlag();
			flag.StartRotating();

			RpcFlagPresentEffect();

			flagPresent = true;
		}
	}
}