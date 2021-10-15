using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Powerup.Powerups
{
	public abstract class PowerupBase : MonoBehaviour
	{
		protected int ammo;
		protected float fireRate;

		[SerializeField]protected float fireCooldown = 0.2f;
		private float nextFire = 0f;

		protected List<Transform> hardpoints = new List<Transform>();

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
		
		public void CmdUse()
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

		protected virtual void Execute() { }
	}
}