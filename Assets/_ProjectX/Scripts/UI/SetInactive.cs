using UnityEngine;

namespace UI
{
	public class SetInactive : MonoBehaviour
	{
		public void ExecuteSetInactive()
		{
			gameObject.SetActive(false);
		}
	}
}