using Data.Enums;

namespace Game.Explosions
{
	public class RocketExplosion : ExplosionBase
	{
		protected override void Awake()
		{
			base.Awake();

			currentPoolType = ObjectPoolType.RocketExplosion;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (isServer)
				ProcessAOE();
		}
	}
}