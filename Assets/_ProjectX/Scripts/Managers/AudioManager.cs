﻿using Mirror;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace Managers
{
	public class AudioManager : NetworkBehaviour
	{
		private static bool hasBeenProvided;
		
		private void Awake()
		{
			if (!hasBeenProvided)
			{
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