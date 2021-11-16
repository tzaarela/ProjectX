using System;
using System.Collections.Generic;
using Mirror;
using Player;
using UnityEngine;

namespace Powerup.Powerups
{
	public abstract class PowerupBase : MonoBehaviour
	{
		[SerializeField]protected float forwardSpawnOffset;
		[SerializeField]protected float heightSpawnOffset;
		[SerializeField]protected int ammo;
		[SerializeField]protected float fireCooldown;
		private float nextFire = 0f;

		protected PlayerController playerController;
		
		protected List<Transform> hardpoints = new List<Transform>();
		
		protected virtual void Start()
		{
			playerController = GetComponentInParent<PlayerController>();
			
			foreach (Transform hardpoint in transform)
			{
				if (hardpoint.CompareTag("Hardpoint"))
				{
					hardpoints.Add(hardpoint);
				}
			}
		}

		public virtual void LocalUse()
		{
			
		}
		
		[Server]
		public void Use(int netID)
		{
			if (nextFire > Time.time)
				return;
			
			nextFire = fireCooldown + Time.time;

			Execute(netID);
		}

		public int GetAmmo()
		{
			return ammo;
		}

		protected virtual void Execute(int netID) { }
	}
}