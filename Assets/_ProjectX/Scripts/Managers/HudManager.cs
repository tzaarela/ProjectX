using UnityEngine;

namespace Managers
{
	public class HudManager : MonoBehaviour
	{
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