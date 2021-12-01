using Mirror;
using System;
using UnityEngine;
using Player;
using DG.Tweening;
using System.Collections;
using Managers;
using System.Collections.Generic;
using System.Linq;

namespace Game.Flag
{
	public class Flag : NetworkBehaviour
	{

		public float dropUninteractableTime = 2f;
		public float dropUpwardsForce = 3;
		public float flagDropOffset = 10f;

		private bool allowedToPickup;

		public Action onFlagPickedUp;
		public Action onFlagDropped;

		[SerializeField] private BoxCollider pickupCollider;
		[SerializeField] private BoxCollider physicsCollider;

		private List<Renderer> renderers; 
		private Rigidbody rb;
		private Tweener rotationTween;
		private float blinkTime;

		private void OnEnable()
		{
			allowedToPickup = true;
		}

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
			renderers = GetComponentsInChildren<Renderer>().ToList();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!isServer)
				return;

			if (!allowedToPickup)
				return;

			if (other.gameObject.CompareTag("Player"))
			{
				PlayerController player = other.gameObject.GetComponent<PlayerController>();
				if (player.GetComponent<Health>().IsDead)
					return;

				allowedToPickup = false;
				
				PickUp(player);
			}
		}

		[ClientRpc]
		public void RpcStartResetBlinking(float resetTime)
		{
			blinkTime = 0;
			StartCoroutine(CoResetBlinking(resetTime));
		}

		private IEnumerator CoResetBlinking(float resetTime)
		{
			while (resetTime > blinkTime)
			{
				yield return new WaitForSeconds(0.2f);
				renderers.ForEach(x => x.enabled = !x.enabled);
				blinkTime += Time.deltaTime;
			}

			renderers.ForEach(x => x.enabled = true);

			yield return null;
		}

		public void TogglePhysics(bool turnOn) 
		{
			physicsCollider.enabled = turnOn;
		}

		public void TogglePickup(bool turnOn)
		{
			pickupCollider.enabled = turnOn;
		}

		[Server]
		public void PickUp(PlayerController playerPickingUp)
		{
			ServiceLocator.ScoreManager.InitializeScoring(playerPickingUp.playerName);
			playerPickingUp.TakeFlag(this);
			onFlagPickedUp();
			GetComponent<NetworkTransform>().enabled = false;
			RpcDeactivateFlag();
		}
		
		[Server]
		public void Drop(Vector3 position, Vector3 relativeVelocity)
		{
			ServiceLocator.ScoreManager.StopScoreCounter();
			TogglePickup(false);
			transform.position = position + Vector3.up * flagDropOffset;
			physicsCollider.enabled = true;
			rb.isKinematic = false;
			gameObject.SetActive(true);
			rb.velocity = relativeVelocity;
			rb.AddForce(Vector3.up * dropUpwardsForce);
			GetComponent<NetworkTransform>().enabled = true;
			RpcActivateFlag();
			onFlagDropped();
			StartCoroutine(CoWaitForInteractable());
		}

		private IEnumerator CoWaitForInteractable()
		{
			yield return new WaitForSeconds(dropUninteractableTime);
			TogglePickup(true);
		}

		[ClientRpc]
		public void RpcDeactivateFlag() 
		{
			gameObject.SetActive(false);
		}

		[ClientRpc]
		public void RpcActivateFlag()
		{
			allowedToPickup = true;
			renderers.ForEach(x => x.enabled = true);
			gameObject.SetActive(true);
		}
		
		public void StartRotating()
		{
			rotationTween = transform.DOLocalRotate(new Vector3(0, 180, 0), 2f, RotateMode.Fast)
				.SetLoops(-1, LoopType.Incremental)
				.SetEase(Ease.Linear);
		}

		public void StopRotating()
		{
			if (rotationTween != null)
				rotationTween.Kill();
		}
	}
}