using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class PreventDeselection : MonoBehaviour
	{
		EventSystem evt;

		private void Start()
		{
			evt = EventSystem.current;
		}

		GameObject sel;

		private void Update()
		{
			if (evt.currentSelectedGameObject != null && evt.currentSelectedGameObject != sel)
				sel = evt.currentSelectedGameObject;
			else if (sel != null && evt.currentSelectedGameObject == null)
				evt.SetSelectedGameObject(sel);
		}
	}
}