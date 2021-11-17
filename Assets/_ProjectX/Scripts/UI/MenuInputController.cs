using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI
{
	public class MenuInputController : MonoBehaviour
	{
		[SerializeField] private InputActionReference navigateInput;
		[SerializeField] private InputActionReference pointerInput;
		[SerializeField] private InputActionReference submitInput;

		private GameObject previousSelection;
		
		private EventSystem eventSystem;

		private bool navigationIsActivated;

		private void Start()
		{
			pointerInput.action.Enable();
			pointerInput.action.performed += PointerPerformed;
			
			navigateInput.action.Enable();
			// navigateInput.action.started += NavigateStarted;
			navigateInput.action.canceled += NavigateCanceled;
			
			submitInput.action.Enable();
			submitInput.action.started += SubmitStarted;
			submitInput.action.canceled += SubmitCanceled;

			eventSystem = EventSystem.current;
			previousSelection = eventSystem.firstSelectedGameObject;
			navigationIsActivated = true;
		}

		private void SubmitStarted(InputAction.CallbackContext obj)
		{
			// print("SUBMIT STARTED");
			// print(obj.ReadValueAsButton());
			// print(eventSystem.currentSelectedGameObject);

			if (eventSystem.currentSelectedGameObject != null)
			{
				previousSelection = eventSystem.currentSelectedGameObject;
			}
			
			// if (eventSystem.currentSelectedGameObject.TryGetComponent(out Button button))
			// {
			// 	button.onClick.Invoke();
			// 	// button.interactable = false;
			// 	// eventSystem.SetSelectedGameObject(previousSelection);
			// }
		}

		private void SubmitCanceled(InputAction.CallbackContext obj)
		{
			// print("SUBMIT CANCELED");
			// print(obj.ReadValueAsButton());
			// print(eventSystem.currentSelectedGameObject);
			
			if (eventSystem.currentSelectedGameObject == null)
			{
				eventSystem.SetSelectedGameObject(previousSelection);
			}
		}

		private void PointerPerformed(InputAction.CallbackContext obj)
		{
			if (!navigationIsActivated)
				return;

			// print("POINTER");
			eventSystem.SetSelectedGameObject(null);
			navigationIsActivated = false;
		}

		private void NavigateStarted(InputAction.CallbackContext obj)
		{
			// print("NAVIGATE STARTED");
			// print(obj.ReadValueAsObject());
			// print(eventSystem.currentSelectedGameObject);
			
			if (!navigationIsActivated)
				return;

			if (eventSystem.currentSelectedGameObject != null)
			{
				previousSelection = eventSystem.currentSelectedGameObject;
			}
		}

		private void NavigateCanceled(InputAction.CallbackContext obj)
		{
			// print("NAVIGATE CANCELED");
			// print(obj.ReadValueAsObject());
			// print(eventSystem.currentSelectedGameObject);

			if (navigationIsActivated)
				return;

			if (eventSystem.currentSelectedGameObject == null)
			{
				eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
				navigationIsActivated = true;
			}
		}
	}
}