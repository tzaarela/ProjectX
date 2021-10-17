using Mirror;

namespace Managers
{
	public class HudManager : NetworkBehaviour
	{
		// NetworkIdentity = !ServerOnly
		private void Awake()
		{
			print("HudManager provided to ServiceLocator");
			ServiceLocator.ProvideHudManager(this);
		}
		
		private void OnDestroy()
		{
			ServiceLocator.ProvideHudManager(null);
		}
	}
}