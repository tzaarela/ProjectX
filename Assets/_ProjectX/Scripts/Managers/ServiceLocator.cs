using UnityEngine;

namespace Managers
{
	public static class ServiceLocator
	{
		private static RoundManager roundManager;
		private static AudioManager audioManager;
		private static ObjectPoolsManager objectPoolsManager;
		
		public static RoundManager RoundManager
		{
			get
			{
				if( roundManager == null )
				{
					Debug.LogError("GameManager is null!");
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
		
		public static AudioManager ObjectPools
		{
			get
			{
				if( audioManager == null )
				{
					Debug.LogError("ObjectPoolsManager is null!");
				}
				return audioManager;
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
		
		public static void ProvideObjectPoolsManager(ObjectPoolsManager objectPool)
		{
			objectPoolsManager = objectPool;
		}
	}
}