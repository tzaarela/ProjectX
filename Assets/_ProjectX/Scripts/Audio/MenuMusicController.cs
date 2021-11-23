using Mirror;
using UnityEngine;

namespace Audio
{
	public class MenuMusicController : MonoBehaviour
	{
		private static bool isRunning;
		
		private void Awake()
		{
			if (!isRunning)
			{
				isRunning = true;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}
		
		public void Destroy()
		{
			isRunning = false;
			Destroy(gameObject);
		}
	}
}