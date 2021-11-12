using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Data.Containers
{
	public class AoeData
	{
		public Vector3 direction;
		public float distance;
		public float rotationEffect;
		public float upwardEffect;
		public int damage;
		public int spawnedById;
		public bool shouldRotate = false;

		public AoeData(Vector3 direction, float distance, int damage, int spawnedById, bool shouldRotate = false, float rotationEffect = 1, float upwardEffect = 1)
		{
			this.direction = direction;
			this.distance = distance;
			this.damage = damage;
			this.spawnedById = spawnedById;
			this.shouldRotate = shouldRotate;
			this.rotationEffect = rotationEffect;
			this.upwardEffect = upwardEffect;
		}
	}
}
