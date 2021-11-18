using System;
using System.Collections;
using _ProjectX.Scripts.Data.ScriptableObjects;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Player
{
	public class CarSetup : NetworkBehaviour
	{
		private InputManager inputManager;
		[SerializeField] private TMPro.TextMeshProUGUI carSettingsText;
		
		public SO_CarSettings settings;
		public bool useSettingsPool;
		public SO_CarSettings[] settingsPool;
		
		public int randomIndex; 
		
		private DriveController driveController;
		private Rigidbody rb;

		private Coroutine CoShowSettingIndex;
		
		private void Awake()
		{
			driveController = GetComponent<DriveController>();
			rb = GetComponent<Rigidbody>();
			
			if (useSettingsPool)
			{
				randomIndex = Random.Range(0, settingsPool.Length);
				settings = settingsPool[randomIndex];
			}

			rb.mass = settings.mass;
			rb.drag = settings.drag;
			rb.angularDrag = settings.angularDrag;
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			
			if(!isServer)
				return;

			inputManager = GetComponent<InputManager>();
			inputManager.playerControls.Player.ShowCarSettingIndex.performed += ShowCarSettingIndexOnperformed;
			inputManager.playerControls.Player.RandomCarSetting.performed += PickNextSetting;
		}
		
		[Server]
		private void ShowCarSettingIndexOnperformed(InputAction.CallbackContext obj)
		{
			if(CoShowSettingIndex == null)
				CoShowSettingIndex = StartCoroutine(CoShowCarSettingsIndex());
		}

		IEnumerator CoShowCarSettingsIndex()
		{
			carSettingsText.text = ""+randomIndex;
			yield return new WaitForSeconds(2);
			carSettingsText.text = "";
			CoShowSettingIndex = null;
		}
		
		[Server]
		public void PickNextSetting(InputAction.CallbackContext obj)
		{
			if (!useSettingsPool)
				return;
			
			if (randomIndex + 1 > settingsPool.Length - 1)
			{
				randomIndex = 0;
			}
			else
			{
				randomIndex++;
			}

			settings = settingsPool[randomIndex];
			UpdateCar();
		}
		
		public void UpdateCar()
		{
			rb = GetComponent<Rigidbody>();
			
			rb.mass = settings.mass;
			rb.drag = settings.drag;
			rb.angularDrag = settings.angularDrag;
			
			driveController.SetupCarSettings();
		}
	}
}