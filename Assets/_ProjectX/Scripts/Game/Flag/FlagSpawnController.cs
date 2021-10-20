using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Flag
{
	public class FlagSpawnController : NetworkBehaviour
	{
		[SerializeField] private Flag flag;
		[SerializeField] private Transform flagSlot;
		[SerializeField] private GameObject flagPresentEffect;
		[SerializeField] private GameObject flagNotPresentEffect;

		public bool flagPresent = true;

		public override void OnStartServer()
		{
			flag.onFlagPickedUp += HandleOnFlagPickedUp;
		}

		[Server]
		private void HandleOnFlagPickedUp()
		{
			flagPresent = false;
			RpcFlagNotPresentEffect();
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
		public void DespawnFlag()
		{
			flag.gameObject.SetActive(false);

			RespawnFlag();
		}

		[Server]
		public void RespawnFlag()
		{
			flag.GetComponent<Rigidbody>().isKinematic = true;
			flag.transform.rotation = Quaternion.identity;
			flag.transform.position = flagSlot.position;
			flag.RpcActivateFlag();
			RpcFlagPresentEffect();
			flagPresent = true;
		}
	}
}