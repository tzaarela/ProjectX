using System;
using Data.Enums;
using Managers;
using UnityEngine;

namespace PowerUp.Projectiles
{
	public class Bullet : ProjectileBase
	{
		public override void SetupProjectile(Vector3 dir)
		{
			base.SetupProjectile(dir);
			
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
		}

		private void OnCollisionEnter(Collision other)
		{
			//ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
	}
}