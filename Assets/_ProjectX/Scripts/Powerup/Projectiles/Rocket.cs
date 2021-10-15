using UnityEngine;

namespace PowerUp.Projectiles
{
	public class Rocket : ProjectileBase
	{
		public override void SetupProjectile(Vector3 dir)
		{
			base.SetupProjectile(dir);
			
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
		}
	}
}