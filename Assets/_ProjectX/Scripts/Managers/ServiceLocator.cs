using UnityEngine;

namespace Managers
{
	public static class ServiceLocator
	{
		private static RoundManager roundManager;
		private static AudioManager audioManager;
		private static HudManager hudManager;
		private static ScoreManager scoreManager;
		private static ObjectPoolsManager objectPoolsManager;
		private static LobbyManager lobbyManager;

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
		public static ScoreManager ScoreManager
		{
			get
			{
				if( scoreManager == null )
				{
					Debug.LogError("ScoreManager is null!");
				}
				return scoreManager;
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

		public static LobbyManager LobbyManager
		{
			get
			{
				if (lobbyManager == null)
				{
					Debug.LogError("RoundManager is null!");
				}
				return lobbyManager;
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
		
		public static void ProvideScoreManager(ScoreManager score)
		{
			scoreManager = score;
		}

		public static void ProvideObjectPoolsManager(ObjectPoolsManager objectPool)
		{
			objectPoolsManager = objectPool;
		}
		public static void ProvideLobbyManager(LobbyManager lobby)
		{
			lobbyManager = lobby;
		}

	}
}