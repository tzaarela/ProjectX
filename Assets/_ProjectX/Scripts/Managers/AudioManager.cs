using Mirror;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace Managers
{
	public class AudioManager : NetworkBehaviour
	{
		private void Awake()
		{
			ServiceLocator.ProvideAudioManager(this);
			DontDestroyOnLoad(gameObject);
		}

		public void PlayClip(AudioType type, AudioSource source)
		{
			
		}
	}
}