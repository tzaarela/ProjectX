using Mirror;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace Managers
{
	public class AudioManager : MonoBehaviour
	{
		private static bool hasBeenProvided;
		
		private void Awake()
		{
			if (!hasBeenProvided)
			{
				print("AudioManager provided to ServiceLocator");
				ServiceLocator.ProvideAudioManager(this);
				hasBeenProvided = true;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public void PlayClip(AudioType type, AudioSource source)
		{
			
		}
	}
}