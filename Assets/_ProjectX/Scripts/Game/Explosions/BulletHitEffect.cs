using Data.Enums;
using Game.Particles;

namespace Game.Explosions
{
	public class BulletHitEffect : SpawnParticleBase
	{
		protected override void Awake()
		{
			base.Awake();

			currentPoolType = ObjectPoolType.BulletHitEffect;
		}
	}
}