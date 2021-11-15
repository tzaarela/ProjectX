using System;
using Mirror;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Data.ScriptableObjects;
using Managers;
using UnityEngine;
using Game.Flag;
using UnityEngine.InputSystem;
using Random = System.Random;
using Data.Containers;
using FMOD.Studio;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal, IReceiveDamageAOE
	{
		[Header("References")]
		[SerializeField] private GameObject flagOnRoof;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private TMPro.TextMeshProUGUI playerNameText;
		[SerializeField] private SO_CarMaterials carMaterials;
		[SerializeField] private PlayerSound playerSound;

		[Header("DeathSettings")] 
		[SerializeField] private ParticleSystem deathFX;
		[SerializeField] private ParticleSystem deathFX2;
		[SerializeField] private ParticleSystem deathSmoke;
		
		[Header("Debug")]
		[SyncVar(hook = nameof(FlagStateChanged))]
		public bool hasFlag;
		
		[SyncVar(hook = nameof(PlayerNameChanged))]
		public string playerName;

		[SyncVar(hook = nameof(PlayerMaterialIndex))]
		public int playerMaterialIndex;

		public Color playerColor;

		[HideInInspector]
		public Rigidbody rb;
		
		private Flag flag;
		private InputManager inputManager;
		private Health health;

		private int playerId;
		public int PlayerId => playerId;

		public bool localPlayer;
		
		[SerializeField]
		[FMODUnity.EventRef]
		private string crashSound;
		private FMOD.Studio.EventInstance crashSoundInstance;

		private void Awake()
		{
			inputManager = GetComponent<InputManager>();
			health = GetComponent<Health>();
			
			crashSoundInstance = FMODUnity.RuntimeManager.CreateInstance(crashSound);
			crashSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, rb));
		}

		[Server]
		public override void OnStartServer()
		{
			rb = GetComponent<Rigidbody>();
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnStartClient()
		{
			if (!isLocalPlayer)
				return;

			rb = GetComponent<Rigidbody>();
			localPlayer = true;
			playerNameText.gameObject.SetActive(false);
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			CmdUpdateActivePlayersList();
			name += "-local";
			SendGlobal(GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME, new GameObjectData(gameObject));
		}
		
		private void Update()
		{
			if (!isLocalPlayer)
				return;

			// DEBUG: F-Key resets car-rotation (when turned over)
			if (Keyboard.current.fKey.wasPressedThisFrame)
			{
				CmdFlipCar();
			}
			
			// DEBUG: I-Key = InstantDeath
			if (Keyboard.current.iKey.wasPressedThisFrame)
			{
				CmdDeath();
			}
		}

		[Server]
		public void TakeFlag(Flag flag)
		{
			this.flag = flag;
			hasFlag = true;
			ServiceLocator.HudManager.UpdateFlagIndicatorTarget(flagHasBeenTaken: true, gameObject);
		}

		[ContextMenu("Drop Flag")]
		[Server]
		public void DropFlag()
		{
			if (!hasFlag)
				return;

			hasFlag = false;
			flag.Drop(transform.position, rb.velocity);
			ServiceLocator.HudManager.UpdateFlagIndicatorTarget(flagHasBeenTaken: false, null);
		}

		//SyncVar Hook
		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
		}

		//SyncVar Hook
		[Client]
		private void PlayerNameChanged(string oldValue, string newValue)
		{
			playerNameText.text = newValue;
		}

		// SyncVar Hook
		 [Client]
		 private void PlayerMaterialIndex(int oldValue, int newValue)
		 {
			 meshRenderer.material = carMaterials.GetMaterial(newValue);
		     playerColor = carMaterials.GetColor(newValue);
		 }

		[Command]
		private void CmdUpdateActivePlayersList()
		{
			ServiceLocator.RoundManager.AddActivePlayer(playerName);
		}

		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}

		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			switch (eventState)
			{
				case GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME:
					RpcEnableAllPlayersInput();
					break;
				
				case GlobalEvent.END_GAMESTATE:
					RpcSetPlayerEndState();
					break;
			}
		}

		[ClientRpc]
		private void RpcEnableAllPlayersInput()
		{
			if (!isLocalPlayer)
				return;

			inputManager.EnableInput();
		}

		[ClientRpc]
		private void RpcSetPlayerEndState()
		{
			inputManager.DisableInput();
			GetComponent<DriveController>().enabled = false;
			GetComponent<PlayerSound>().StopEmitter();

			if (!isServer)
				return;
			
			rb.velocity = Vector3.zero;
		}

		[Client]
		public void Death()
		{
			FMODUnity.RuntimeManager.PlayOneShot("event:/Vehicles/DeathExplosion", Camera.main.transform.position);
			deathFX.Play();
			deathFX2.Play();
			deathSmoke.Play();
			meshRenderer.material.color = Color.black;
			GetComponent<PlayerSound>().StopEmitter();
			
			if (!isLocalPlayer)
				return;
			
			inputManager.DisableInput();
		}

		[Command]
		public void CmdRespawnPlayer()
		{
			health.ResetCurrentHealth();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			FlipCar();
			GetComponent<PowerupController>().Drop();
			RpcRespawnPlayer();
		}
		
		[ClientRpc]
		private void RpcRespawnPlayer()
		{
			deathSmoke.Stop();
			meshRenderer.material.color = Color.white;
			GetComponent<PlayerSound>().PlayEmitter();

			if (!isLocalPlayer)
				return;

			inputManager.EnableInput();
		}
		
		[Server]
		private void FlipCar()
		{
			Vector3 newRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
			transform.rotation = Quaternion.Euler(newRotation);
			
			rb.AddForce(Vector3.up * 5000, ForceMode.Impulse);
			rb.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 5000, ForceMode.Impulse);

			if (transform.position.y > 2f)
			{
				transform.position = new Vector3(transform.position.x,2f, transform.position.z);
				rb.velocity = Vector3.zero;
			}
		}

		// TEMP!
		[Command]
		private void CmdDeath()
		{
			RpcDeath();
			DropFlag();
			ServiceLocator.HudManager.TargetActivateDeathTexts(connectionToClient, "Mr.Debug");
		}

		// TEMP!
		[ClientRpc]
		private void RpcDeath()
		{
			health.IsDead = true;
			Death();
		}

		// TEMP!
		[Command]
		private void CmdFlipCar()
		{
			//rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			FlipCar();
		}

		public void ReceiveDamageAOE(AoeData aoeData)
		{
			rb.AddForce(aoeData.direction * aoeData.distance + Vector3.up * aoeData.upwardEffect, ForceMode.Impulse);
			
			if (aoeData.shouldRotate)
				rb.AddRelativeTorque(aoeData.direction * aoeData.distance * aoeData.rotationEffect, ForceMode.Impulse);
			
			health.ReceiveDamage(50, aoeData.spawnedById);
		}

		private void OnCollisionEnter(Collision other)
		{
			if (!isServer)
				return;
			
			if (other.transform.CompareTag("Obstacle"))
			{
				// if (rb.velocity.magnitude > 0.1f)
				// 	return;
				
				//Vector3.Dot(other.contacts[0].normal,other.relativeVelocity) * rb.mass

				if (other.impulse.magnitude < 500f)
					return;
				
				//Debug.Log("FORCE MAG: " + other.impulse.magnitude);

				Vector3 pushForce = Vector3.ClampMagnitude(other.impulse, 12000);
				
				ForcePush(pushForce);
			}
		}

		[Server]
		private void ForcePush(Vector3 force)
		{
			//Debug.Log("FORCE PUSH: " + force);
			Debug.DrawRay(transform.position, force, Color.red, 0.1f);
			rb.AddForce(force, ForceMode.Impulse);
			RpcPlayCrashSound();
		}
		
		[ClientRpc]
		private void RpcPlayCrashSound()
		{
			crashSoundInstance.stop(STOP_MODE.IMMEDIATE);
			crashSoundInstance.start();
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(crashSoundInstance, transform);
		}
	}
}