using UnityEngine;

namespace Managers
{
	public static class ServiceLocator
	{
		private static RoundManager roundManager;
		private static AudioManager audioManager;
		private static HudManager hudManager;
		private static ObjectPoolsManager objectPoolsManager;
		
		public static RoundManager RoundManager
		{
			get
			{
				if( roundManager == null )
				{
					Debug.LogError("RoundManager is null!");
				}
				return roundManager;
			}
		}
		
		public static AudioManager AudioManager
		{
			get
			{
				if( audioManager == null )
				{
					Debug.LogError("AudioManager is null!");
				}
				return audioManager;
			}
		}
		
		public static HudManager HudManager
		{
			get
			{
				if( hudManager == null )
				{
					Debug.LogError("HudManager is null!");
				}
				return hudManager;
			}
		}
		
		public static ObjectPoolsManager ObjectPools
		{
			get
			{
				if( objectPoolsManager == null )
				{
					Debug.LogError("ObjectPoolsManager is null!");
				}
				return objectPoolsManager;
			}
		}

		public static void ProvideRoundManager(RoundManager round)
		{
			roundManager = round;
		}
		
		public static void ProvideAudioManager(AudioManager audio)
		{
			audioManager = audio;
		}
		
		public static void ProvideHudManager(HudManager hud)
		{
			hudManager = hud;
		}
		
		public static void ProvideObjectPoolsManager(ObjectPoolsManager objectPool)
		{
			objectPoolsManager = objectPool;
		}
	}
}