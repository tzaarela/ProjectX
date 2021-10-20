using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Player;
using DG.Tweening;
using System.Collections;

namespace Game.Flag
{
	public class Flag : NetworkBehaviour
	{

		public float dropInvulnerableTime = 2f;
		public float dropUpwardsForce = 3;
		public float flagDropOffset = 10f;

		public Action onFlagPickedUp;
		public Action onFlagDropped;

		[SerializeField] private BoxCollider pickupCollider;
		[SerializeField] private BoxCollider physicsCollider;
		
		private Rigidbody rb;
		private Tweener rotationTween;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!isServer)
			{
				return;
			}

			if (other.gameObject.CompareTag("Player"))
			{
				PlayerController player = other.gameObject.GetComponent<PlayerController>();
				PickUp(player);
			}
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
			playerPickingUp.GiveFlag(this);
			onFlagPickedUp();
			RpcDeactivateFlag();
		}
		
		[Server]
		public void Drop(Vector3 position, Vector3 relativeVelocity)
		{
			TogglePickup(false);
			transform.position = position + Vector3.up * flagDropOffset;
			physicsCollider.enabled = true;
			rb.isKinematic = false;
			gameObject.SetActive(true);
			rb.velocity = relativeVelocity;
			rb.AddForce(Vector3.up * dropUpwardsForce);
			RpcActivateFlag();
			onFlagDropped();
			StartCoroutine(CoWaitForFlagInvulnerable());
		}

		private IEnumerator CoWaitForFlagInvulnerable()
		{
			yield return new WaitForSeconds(dropInvulnerableTime);
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
			gameObject.SetActive(true);
		}


		public void StartRotating()
		{
			rotationTween = transform.DOLocalRotate(new Vector3(0, 180, 0), 2f, RotateMode.Fast).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
		}

		public void StopRotating()
		{
			if (rotationTween != null)
				rotationTween.Kill();
		}
	}
}
