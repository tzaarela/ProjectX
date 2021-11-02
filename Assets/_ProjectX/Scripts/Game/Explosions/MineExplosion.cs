using Data.Enums;

namespace Game.Explosions
{
	public class MineExplosion : ExplosionBase
	{
		protected override void Awake()
		{
			base.Awake();

			currentPoolType = ObjectPoolType.MineExplosion;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			
			ProcessAOE();
		}
	}
}