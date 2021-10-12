using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio.ScriptableObjects
{
	[CreateAssetMenu]
	public class AudioEvent : ScriptableObject
	{
		public AudioClip[] clips;

		public RangedFloat volume = new RangedFloat(1f, 1f);

		[MinMaxRange(0, 2)] public RangedFloat pitch = new RangedFloat(1f, 1f);

		public void Play(AudioSource audioSource)
		{
			if (clips.Length == 0)
			{
				return;
			}
			
			audioSource.clip   = clips[Random.Range(0, clips.Length)];
			audioSource.volume = Random.Range(volume.minValue, volume.maxValue);
			audioSource.pitch  = Random.Range(pitch.minValue,  pitch.maxValue);
			audioSource.Play();
		}

		public void PlayOneShot(AudioSource audioSource)
		{
			if (clips.Length == 0)
			{
				return;
			}

			audioSource.pitch = Random.Range(pitch.minValue, pitch.maxValue);
			audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], Random.Range(volume.minValue, volume.maxValue));
		}
	}
}