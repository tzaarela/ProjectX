using UnityEngine;

namespace Managers
{
	public static class ServiceLocator
	{
		private static RoundManager roundManager;
		private static AudioManager audioManager;
		
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

		public static void ProvideRoundManager(RoundManager round)
		{
			if (roundManager == null)
			{
				roundManager = round;
			}
		}
		
		public static void ProvideAudioManager(AudioManager audio)
		{
			if (audioManager == null)
			{
				audioManager = audio;
			}
		}
	}
}