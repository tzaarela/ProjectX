using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Powerup.Powerups
{
	public abstract class PowerupBase : MonoBehaviour
	{
		[SerializeField]protected int ammo;
		protected float fireRate;

		[SerializeField]protected float fireCooldown = 0.2f;
		private float nextFire = 0f;

		protected List<Transform> hardpoints = new List<Transform>();

		//public Action OnAmmoDepleted;
		
		protected virtual void Start()
		{
			foreach (Transform hardpoint in transform)
			{
				if (hardpoint.CompareTag("Hardpoint"))
				{
					hardpoints.Add(hardpoint);
				}
			}
		}
		
		[Server]
		public void Use()
		{
			if (nextFire > Time.time)
				return;
			
			nextFire = fireCooldown + Time.time;

			Execute();
		}

		public int GetAmmo()
		{
			return ammo;
		}

		protected virtual void Execute() 
		{
			if (ammo <= 0)
			{
				//OnAmmoDepleted.Invoke();
			}
		}
	}
}