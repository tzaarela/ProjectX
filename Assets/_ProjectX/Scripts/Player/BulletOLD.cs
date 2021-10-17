using Mirror;
using System.Collections;
using Data.Enums;
using Managers;
using UnityEngine;

namespace Player
{
	public class BulletOLD : NetworkBehaviour
	{
		public float shootingStrength = 100000f;
		public float aliveTime = 3f;

		private Rigidbody rb;
		private Vector3 direction;

		public override void OnStartServer()
		{
			base.OnStartServer();
			rb = GetComponent<Rigidbody>();
		}

		[Server]
		public void Shoot(Vector3 dir)
		{
			direction = dir;
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
			StartCoroutine(CoDestroyAfterTime());
		}

		private IEnumerator CoDestroyAfterTime()
		{
			yield return new WaitForSeconds(aliveTime);
			rb.velocity = Vector3.zero;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
	}
}